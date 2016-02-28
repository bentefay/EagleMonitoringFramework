using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Windows.CredentialManagement
{
    public abstract class BaseCredentialsPrompt : ICredentialsPrompt
    {
        private bool _disposed;
        private static readonly SecurityPermission _unmanagedCodePermission;
        private static readonly object _lockObject = new object();

        private string _username;
        private SecureString _password;
        private bool _saveChecked;
        private string _message;
        private string _title;
        private int _errorCode;

        static BaseCredentialsPrompt()
        {
            lock (_lockObject)
            {
                _unmanagedCodePermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            }
        }

        protected void CheckNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("CredentialsPrompt object is already disposed.");
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~BaseCredentialsPrompt()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            _disposed = true;
        }

        protected void AddFlag(bool add, int flag)
        {
            if (add)
            {
                DialogFlags |= flag;
            }
            else
            {
                DialogFlags &= ~flag;
            }
        }

        protected virtual NativeMethods.CREDUI_INFO CreateCREDUI_INFO(IntPtr owner)
        {
            var credUi = new NativeMethods.CREDUI_INFO();
            credUi.cbSize = Marshal.SizeOf(credUi);
            credUi.hwndParent = owner;
            credUi.pszCaptionText = Title;
            credUi.pszMessageText = Message;
            return credUi;
        }

        public bool SaveChecked
        {
            get
            {
                CheckNotDisposed();
                return _saveChecked;
            }
            set
            {
                CheckNotDisposed();
                _saveChecked = value;
            }
        }

        public string Message
        {
            get
            {
                CheckNotDisposed();
                return _message;
            }
            set
            {
                CheckNotDisposed();
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Message));

                if (value.Length > NativeMethods.CREDUI_MAX_MESSAGE_LENGTH)
                    throw new ArgumentOutOfRangeException(nameof(Message));

                _message = value;
            }
        }

        public string Title
        {
            get
            {
                CheckNotDisposed();
                return _title;
            }
            set
            {
                CheckNotDisposed();
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Title));

                if (value.Length > NativeMethods.CREDUI_MAX_CAPTION_LENGTH)
                    throw new ArgumentOutOfRangeException(nameof(Title));

                _title = value;
            }
        }

        public string Username
        {
            get
            {
                CheckNotDisposed();
                return _username ?? string.Empty;
            }
            set
            {
                CheckNotDisposed();
                if (null == value)
                    throw new ArgumentNullException(nameof(Username));

                if (value.Length > NativeMethods.CREDUI_MAX_USERNAME_LENGTH)
                    throw new ArgumentOutOfRangeException(nameof(Username));

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
                if (null == value)
                    throw new ArgumentNullException(nameof(Password));

                if (value.Length > NativeMethods.CREDUI_MAX_PASSWORD_LENGTH)
                    throw new ArgumentOutOfRangeException(nameof(Password));

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

        public int ErrorCode
        {
            get
            {
                CheckNotDisposed();
                return _errorCode;
            }
            set
            {
                CheckNotDisposed();
                _errorCode = value;
            }
        }

        public abstract bool ShowSaveCheckBox { get; set; }

        public abstract bool GenericCredentials { get; set; }

        protected int DialogFlags { get; private set; }

        public virtual DialogResult ShowDialog() => ShowDialog(IntPtr.Zero);

        public abstract DialogResult ShowDialog(IntPtr owner);
    }
}
