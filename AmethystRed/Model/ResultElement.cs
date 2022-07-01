using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmethystSoftware
{
    public class ResultElement
    {
        public bool OperationSuccess;
        public string OperationMessage;
        public string OperationTitle;
        public ResultElement(bool operationSuccess, string operationMessage)
        {
            this.OperationSuccess = operationSuccess;
            this.OperationMessage = operationMessage;
            if (this.OperationSuccess)
            {
                this.OperationTitle = "Операция успешна";
            }
            else
            {
                this.OperationTitle = "Ошибка";
            }
        }

    }
}