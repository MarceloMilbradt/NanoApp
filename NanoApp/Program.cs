using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NanoApp
{
    public class Program
    {
        private const int LedPin = 2;
        private static GpioController _gpioController = new GpioController();

        public static void Main()
        {
            WifiConnection.SetupAndConnectNetwork();

            var client = new MqttClient("test.mosquitto.org");
            var clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // STEP 3: subscribe to topics you want
            client.Subscribe(new[] { "nf-mqtt/basic-demo" }, new[] { MqttQoSLevel.AtLeastOnce });
            client.MqttMsgPublishReceived += HandleIncomingMessage;

            // STEP 4: publish something and watch it coming back
            for (int i = 0; i < 5; i++)
            {
                client.Publish("nf-mqtt/basic-demo", Encoding.UTF8.GetBytes("===== Hello MQTT! ====="), null, null, MqttQoSLevel.AtLeastOnce, false);
                Thread.Sleep(5000);
            }

            // STEP 5: disconnecting
            client.Disconnect();

            // App must not return.
            Thread.Sleep(Timeout.Infinite);
        }

        private static void HandleIncomingMessage(object sender, MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine($"Message received: {Encoding.UTF8.GetString(e.Message, 0, e.Message.Length)}");
            BlinkLed();
        }

        private static void BlinkLed()
        {
            GpioPin led = _gpioController.OpenPin(LedPin, PinMode.Output);
            led.Toggle();
            Thread.Sleep(125);
            led.Toggle();
            _gpioController.ClosePin(LedPin);
        }
    }
}
