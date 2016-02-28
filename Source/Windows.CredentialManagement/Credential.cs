using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Windows.CredentialManagement
{
    public class Credential: IDisposable
    {
        private static readonly object _lockObject = new object();
        private static readonly SecurityPermission _unmanagedCodePermission;

        private bool _disposed;
        private CredentialType _type;
        private string _target;
        private SecureString _password;
        private string _username;
        private string _description;
        private DateTime _lastWriteTime;
        private PersistanceType _persistanceType;

        static Credential()
        {
            lock (_lockObject)
            {
                _unmanagedCodePermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            }
        }

        public Credential()
        {
            PersistanceType = PersistanceType.Session;
            _lastWriteTime = DateTime.MinValue;
        }

        public Credential(string target, CredentialType type = CredentialType.Generic) : this()
        {
            Target = target;
            Type = type;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~Credential()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SecurePassword.Clear();
                    SecurePassword.Dispose();
                }
            }
            _disposed = true;
        }

        private void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Credential object is already disposed.");
        }

        public string Username {
            get
            {
                CheckNotDisposed();
                return _username;
            }
            set
            {
                CheckNotDisposed();
                _username = value;
            }
        }

        public string Password
        {
            get
            {
                return SecureStringHelper.CreateString(SecurePassword);
            }
            set
            {
                CheckNotDisposed();
                SecurePassword = SecureStringHelper.CreateSecureString(string.IsNullOrEmpty(value) ? string.Empty : value);
            }
        }

        public SecureString SecurePassword
        {
            get
            {
                CheckNotDisposed();
                _unmanagedCodePermission.Demand();
                return _password?.Copy() ?? new SecureString();
            }
            set
            {
                CheckNotDisposed();
                if (null != _password)
                {
                    _password.Clear();
                    _password.Dispose();
                }
                _password = value?.Copy() ?? new SecureString();
            }
        }

        public string Target
        {
            get
            {
                CheckNotDisposed();
                return _target;
            }
            set
            {
                CheckNotDisposed();
                _target = value;
            }
        }

        public string Description
        {
            get
            {
                CheckNotDisposed();
                return _description;
            }
            set
            {
                CheckNotDisposed();
                _description = value;
            }
        }

        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

        public DateTime LastWriteTimeUtc 
        { 
            get
            {
                CheckNotDisposed();
                return _lastWriteTime;
            }
            private set { _lastWriteTime = value; }
        }

        public CredentialType Type
        {
            get
            {
                CheckNotDisposed();
                return _type;
            }
            set
            {
                CheckNotDisposed();
                _type = value;
            }
        }

        public PersistanceType PersistanceType
        {
            get
            {
                CheckNotDisposed();
                return _persistanceType;
            }
            set
            {
                CheckNotDisposed();
                _persistanceType = value;
            }
        }

        public bool Save()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            byte[] passwordBytes = Encoding.Unicode.GetBytes(Password);
            if (Password.Length > (512))
            {
                throw new ArgumentOutOfRangeException("The password has exceeded 512 bytes.");
            }

            var credential = new NativeMethods.CREDENTIAL
            {
                TargetName = Target,
                UserName = Username,
                CredentialBlob = Marshal.StringToCoTaskMemUni(Password),
                CredentialBlobSize = passwordBytes.Length,
                Comment = Description,
                Type = (int) Type,
                Persist = (int) PersistanceType
            };

            var result = NativeMethods.CredWrite(ref credential, 0);
            if (!result)
            {
                return false;
            }
            LastWriteTimeUtc = DateTime.UtcNow;
            return true;
        }

        public bool Delete()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            if (string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Target must be specified to delete a credential.");
            }

            var target = string.IsNullOrEmpty(Target) ? new StringBuilder() : new StringBuilder(Target);
            var result = NativeMethods.CredDelete(target, Type, 0);
            return result;
        }

        public bool Load()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            IntPtr credPointer;

            var result = NativeMethods.CredRead(Target, Type, 0, out credPointer);
            if (!result)
            {
                return false;
            }

            using (var credentialHandle = new NativeMethods.CriticalCredentialHandle(credPointer))
            {
                LoadInternal(credentialHandle.GetCredential());
            }

            return true;
        }

        public bool Exists()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            if (string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Target must be specified to check existance of a credential.");
            }

            using (var existing = new Credential { Target = Target, Type = Type })
            {
                return existing.Load();
            }
        }

        internal void LoadInternal(NativeMethods.CREDENTIAL credential)
        {
            Username = credential.UserName;
            if (credential.CredentialBlobSize > 0)
            {
                Password = Marshal.PtrToStringUni(credential.CredentialBlob, credential.CredentialBlobSize / 2);
            }
            Target = credential.TargetName;
            Type = (CredentialType)credential.Type;
            PersistanceType = (PersistanceType)credential.Persist;
            Description = credential.Comment;
            LastWriteTimeUtc = DateTime.FromFileTimeUtc(credential.LastWritten);
        }
    }
}