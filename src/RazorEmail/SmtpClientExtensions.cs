using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RazorEmail
{
    /// <summary>Extension methods for working with SmtpClient asynchronously.</summary> 
    public static class SmtpClientExtensions
    {
        /// <summary>Sends an e-mail message asynchronously.</summary> 
        /// <param name="smtpClient">The client.</param> 
        /// <param name="message">A MailMessage that contains the message to send.</param> 
        /// <param name="userToken">A user-defined object stored in the resulting Task.</param> 
        /// <returns>A Task that represents the asynchronous send.</returns> 
        public static Task<T> SendTask<T>(this SmtpClient smtpClient, MailMessage message, T userToken)
        {
            return SendTaskCore<T>(smtpClient, userToken, tcs => smtpClient.SendAsync(message, tcs));
        }

        /// <summary>The core implementation of SendTask.</summary> 
        /// <param name="smtpClient">The client.</param> 
        /// <param name="userToken">The user-supplied state.</param> 
        /// <param name="sendAsync"> 
        /// A delegate that initiates the asynchronous send. 
        /// The provided TaskCompletionSource must be passed as the user-supplied state to the actual SmtpClient.SendAsync method. 
        /// </param> 
        /// <returns></returns> 
        private static Task<T> SendTaskCore<T>(SmtpClient smtpClient, T userToken, Action<TaskCompletionSource<T>> sendAsync)
        {
            // Validate we're being used with a real smtpClient.  The rest of the arg validation 
            // will happen in the call to sendAsync. 
            if (smtpClient == null) 
                throw new ArgumentNullException("smtpClient");

            // Create a TaskCompletionSource to represent the operation 
            var tcs = new TaskCompletionSource<T>(userToken);

            // Register a handler that will transfer completion results to the TCS Task 
            SendCompletedEventHandler handler = null;
            handler = (sender, e) => HandleCompletion(tcs, e, () => default(T), () => smtpClient.SendCompleted -= handler);
            smtpClient.SendCompleted += handler;

            // Try to start the async operation.  If starting it fails (due to parameter validation) 
            // unregister the handler before allowing the exception to propagate. 
            try
            {
                sendAsync(tcs);
            }
            catch (Exception exc)
            {
                smtpClient.SendCompleted -= handler;
                tcs.TrySetException(exc);
            }

            // Return the task to represent the asynchronous operation 
            return tcs.Task;
        }

        internal static void HandleCompletion<T>(
            TaskCompletionSource<T> tcs, AsyncCompletedEventArgs e, Func<T> getResult, Action unregisterHandler)
        {
            // Transfers the results from the AsyncCompletedEventArgs and getResult() to the 
            // TaskCompletionSource, but only AsyncCompletedEventArg's UserState matches the TCS 
            // (this check is important if the same WebClient is used for multiple, asynchronous 
            // operations concurrently).  Also unregisters the handler to avoid a leak. 
            if (e.UserState == tcs)
            {
                if (e.Cancelled) tcs.TrySetCanceled();
                else if (e.Error != null) tcs.TrySetException(e.Error);
                else tcs.TrySetResult(getResult());
                unregisterHandler();
            }
        } 
    }
}