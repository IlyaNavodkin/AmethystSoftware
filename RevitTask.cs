using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmethystSoftware
{
    public class RevitTask
    {
        private EventHandler _handler;
        private TaskCompletionSource<object> _tcs;
        private ExternalEvent _externalEvent;

        public RevitTask()
        {
            _handler = new EventHandler();

            _handler.EventCompleted += OnEventCompleted;

            _externalEvent = ExternalEvent.Create(_handler);
        }

        public Task<TResult> Run<TResult>(Func<UIApplication, TResult> func)
        {
            _tcs = new TaskCompletionSource<object>();

            var task = Task.Run(async () => (TResult)await _tcs.Task);

            _handler.Func = (app) => func(app);

            _externalEvent.Raise();

            return task;
        }

        public Task Run(Action<UIApplication> act)
        {
            _tcs = new TaskCompletionSource<object>();

            _handler.Func = (app) => { act(app); return new object(); };

            _externalEvent.Raise();

            return _tcs.Task;
        }

        private void OnEventCompleted(object sender, object result)
        {
            if (_handler.Exception == null)
            {
                _tcs.TrySetResult(result);
            }
            else
            {
                _tcs.TrySetException(_handler.Exception);
            }
        }

        private class EventHandler :
            IExternalEventHandler
        {
            private Func<UIApplication, object> _func;

            public event EventHandler<object> EventCompleted;

            public Exception Exception { get; private set; }

            public Func<UIApplication, object> Func
            {
                get => _func;
                set => _func = value ??
                    throw new ArgumentNullException();
            }

            public void Execute(UIApplication app)
            {
                object result = null;

                Exception = null;

                try
                {
                    result = Func(app);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Внимание!", $"Исключение в методе [{ex.StackTrace}]\nОписание исключения [{ex.Message}]");

                    Exception = ex;
                }

                EventCompleted?.Invoke(this, result);
            }

            public string GetName()
            {
                return "RevitTask";
            }
        }
    }
}






