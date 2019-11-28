using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Booyco_HMI_Utility
{

    class MQTTManagement
    {
        public MqttClient client;

        public void Subscribe(string Topic1, string Topic2)
        {
            client.Subscribe(new string[] { Topic1, Topic2 }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public void Unsubscribe(string Topic)
        {
            client.Unsubscribe(new string[] { Topic });
        }
        // static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        //  {


        // handle message received
        //  }

        public void Broker(string BrokerIp)
        {
            client = new MqttClient(BrokerIp);
        }
   
        public void Client_Connect(string clientId)
        {
            Task.Run(() => PersistConnectionAsync(clientId));
            
        }

        private async Task PersistConnectionAsync(string clientId)
        {
            var connected = client.IsConnected;
            while (!connected)
            {              
                    try
                    {
                        client.Connect(clientId);
                    }
                    catch
                    {

                    }
                
                await Task.Delay(1000);
                connected = client.IsConnected;
            }
        }

        public void Client_Disconnect()
        {
            try
           {
                client.Disconnect();
            }
            catch
            {

            }
       
        }
        void Publish(string Topic, String Message)
    {
        // publish a message on "/home/temperature" topic with QoS 2
        client.Publish(Topic, Encoding.UTF8.GetBytes(Message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

}

class MQTTEntry : INotifyPropertyChanged
{
    private int _number;
    public int Number
    {
        get
        {
            return _number;
        }
        set
        {
            _number = value;
            OnPropertyChanged("Number");
        }
    }

    private DateTime _dateTime;
    public DateTime DateTime
    {
        get
        {
            return _dateTime;
        }
        set
        {
            _dateTime = value;
            OnPropertyChanged("DateTime");
        }
    }



    private string _topic;
    public string Topic
    {
        get
        {
            return _topic;
        }
        set
        {
            _topic = value;
            OnPropertyChanged("Topic");
        }
    }

    private string _data;
    public string Data
    {
        get
        {
            return _data;
        }
        set
        {
            _data = value;
            OnPropertyChanged("Data");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}
}
