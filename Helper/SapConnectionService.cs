using Microsoft.Extensions.Options;
using SAP.Middleware.Connector;

namespace ProjectWBSAPI.Helper
{
    public class SapConnectionService
    {
        private readonly SapSettings _settings;

        public SapConnectionService(IOptions<SapSettings> settings)
        {
            _settings = settings.Value;
        }

        public RfcDestination GetDestination()
        {
            RfcTrace.TraceDirectory="C:\\temp\\NCoTrace.log";
            var config = new RfcConfigParameters
        {
            { RfcConfigParameters.Name, _settings.SAPName },
            { RfcConfigParameters.AppServerHost, _settings.AppServerHost },
            { RfcConfigParameters.SystemNumber, _settings.SAPSystemNumber },
            { RfcConfigParameters.Client, _settings.SAPClient },
            { RfcConfigParameters.User, _settings.SAPUser },
            { RfcConfigParameters.Password, _settings.SAPPassword },
            { RfcConfigParameters.Language, _settings.SAPLanguage },
            { RfcConfigParameters.SAPRouter, _settings.SAPRouter }
        };

            return RfcDestinationManager.GetDestination(config);
        }
    }
}
