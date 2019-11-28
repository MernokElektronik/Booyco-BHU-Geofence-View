using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Booyco_HMI_Utility
{
    class MQTTDataLogs
    {
        string MQTT_BROKER_ADDRESS = "154.0.167.245";
        public MQTTManagement MQTTManager = new MQTTManagement();

        public void MQTT_Init()
        {
            // create client instance
            MQTTManager.Broker(MQTT_BROKER_ADDRESS);                                  

            // MQTT_Subscribe("/Booyco/DataLogs/#SYS/#");

        }

        public void MQTT_Start()
        {
            string clientId = Guid.NewGuid().ToString();
            MQTTManager.Client_Connect(clientId);

            MQTTManager.Subscribe("/Booyco/BHU/+/DataLogs/Analog/", "/Booyco/BHU/+/Parameters/");
        }

      
        public void MQTT_Stop()
        {           
            MQTTManager.Unsubscribe("/Booyco/BHU/+/DataLogs/Analog/");
            MQTTManager.Client_Disconnect();
        }
    }
}
