using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;

namespace ACAfiling_Web.Controllers {
    public class ContractBehavior : IContractBehavior {
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) {
            return;
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime) {
            clientRuntime.MessageInspectors.Add(new SoapMessageInspector());
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime) {
            return;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint) {
            return;
        }
    } // end ContractBehavior class


    // implement endpoint class
    public class EndpointBehavior : IEndpointBehavior {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) {
            return;
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) {
            clientRuntime.MessageInspectors.Add(new SoapMessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) {
            return;
        }

        public void Validate(ServiceEndpoint endpoint) {
            return;
        }
    } // end EndpointBehavior class

    public class EndpointBehaviorExtensionElement : BehaviorExtensionElement {
        public override Type BehaviorType
        {
            get
            {
                return typeof(EndpointBehavior);
            }
        }

        protected override object CreateBehavior() {
            return new EndpointBehavior();
        }
    }
}