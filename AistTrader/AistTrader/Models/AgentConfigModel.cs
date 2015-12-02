using System;
using System.ComponentModel;

namespace AistTrader.Models
{
    class AgentConfigModel : IDataErrorInfo
    {

        public string Error { get; private set; }

        public string AlgorithmName { get; set; }
        

        public void Save()
        {
            //Insert code to save new Product to database etc 
        }

        public string this[string propertyName]
        {
            get
            {
                string validationResult = null;
                switch (propertyName)
                {
                    case "AlgorithmName":
                        validationResult = ValidateName();
                        break;
                    default:
                        throw new ApplicationException("Unknown Property being validated on Product.");
                }
                return validationResult;
            }
        }

        private string ValidateName()
        {
            if (String.IsNullOrEmpty(this.AlgorithmName))
                return "Product Name needs to be entered.";
            else if (this.AlgorithmName.Length < 5)
                return "Product Name should have more than 5 letters.";
            else
                return String.Empty;
        }
    }
}
