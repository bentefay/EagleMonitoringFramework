using System;
using System.Threading;
using System.Threading.Tasks;

namespace Emf.Web.Ui.Models
{
    public static class RepeatTask
    {
        public static async Task Every(Action<CancellationToken> action, TimeSpan interval, CancellationToken token)
        {
            while (true)
            {
                action(token);

                try
                {
                    await Task.Delay(interval, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        public static async Task Every(Func<CancellationToken, Task> action, TimeSpan interval, CancellationToken token)
        {
            while (true)
            {
                await action(token);

                try
                {
                    await Task.Delay(interval, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
    }
}