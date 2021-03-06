# LoRaEngine

A **.NET Standard 2.0** solution with the following projects:

- **modules** - Azure IoT Edge modules.
  - **LoRaWanPktFwdModule** packages the network forwarder into an IoT Edge compatible docker container. See https://github.com/Lora-net/packet_forwarder and https://github.com/Lora-net/lora_gateway.
  - **LoRaWanNetworkSrvModule** - is the LoRaWAN network server implementation.
- **LoraKeysManagerFacade** - An Azure function handling device provisioning (e.g. LoRa network join, OTAA) with Azure IoT Hub as persistence layer.
- **LoRaDevTools** - library for dev tools (git submodule)

## Getting started with: Build and deploy LoRaEngine

The following guide describes the necessary steps to build and deploy the LoRaEngine to an [Azure IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/) installation on a LoRaWAN antenna gateway.

### Used Azure services

- [Azure IoT Hub](https://azure.microsoft.com/en-us/services/iot-hub/)
- [Azure Container registry](https://azure.microsoft.com/en-us/services/container-registry/)
- [Azure Functions](https://azure.microsoft.com/en-us/services/functions/)

### Prerequisites

- Have LoRaWAN concentrator and edge node hardware ready for testing. The LoRaEngine has been tested and build for various hardware setups. However, for this guide we used the [Seeed LoRa/LoRaWAN Gateway Kit](http://wiki.seeedstudio.com/LoRa_LoRaWan_Gateway_Kit/) and concentrator and the [Seeeduino LoRaWAN](http://wiki.seeedstudio.com/Seeeduino_LoRAWAN/) as edge node.
- [Installed Azure IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux-arm) on your LoRaWAN concentrator enabled edge device.
- SetUp an Azure IoT Hub instance and be familiar with [Azure IoT Edge module deployment](https://docs.microsoft.com/en-us/azure/iot-edge/quickstart-linux) mechanism.
- Be familiar with [Azure IoT Edge module development](https://docs.microsoft.com/en-us/azure/iot-edge/quickstart-linux). Note: the following guide expects that your modules will be pushed to [Azure Container registry](https://azure.microsoft.com/en-us/services/container-registry/).

### SetUp Azure function facade and [Azure Container registry](https://azure.microsoft.com/en-us/services/container-registry/)

- Deploy the [function](LoraKeysManagerFacade). In VSCode with the [functions plugin](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions) you can run the command `Azure Functions: Deploy to function app...`. Then you have to select the folder `LoraKeysManagerFacade/bin/Release/netstandard2.0/publish` (unfortunately at time of this writing we saw the behavior that VSCode is proposing the wrong folder) and select for the environment `C#` in version `beta`.

- Configure IoT Hub access key in the function:

Copy `Connection string` with owner policy applied:

![Copy IoT Hub Connection string](/pictures/CopyIoTHubString.PNG)

Now paste it into `Application settings` -> `Connection strings` as `IoTHubConnectionString`:

![Paste IoT Hub Connection string](/pictures/FunctionPasteString.PNG)

- Extract Facade function `Host key` (needed in next step)

![Extract Facade function Host key](/pictures/FunctionHostKey.PNG)

- Configure your `.env` file with your [Azure Container registry](https://azure.microsoft.com/en-us/services/container-registry/) as well as the Facade access URL and credentials. Those variables will be used by our [Azure IoT Edge solution template](/LoRaEngine/deployment.template.json)

```{bash}
CONTAINER_REGISTRY_USERNAME=myregistryrocks
CONTAINER_REGISTRY_PASSWORD=ghjGD5jrK6667
CONTAINER_REGISTRY_ADDRESS=myregistryrocks.azurecr.io
FACADE_SERVER_URL=https://lorafacadefunctionrocks.azurewebsites.net/api/
FACADE_AUTH_CODE=yourFunctionHostKey
```

### SetUp concentrator with Azure IoT Edge

- Note: if your LoRa chip set is connected by SPI on raspberry PI bus don't forget to [enable it](https://www.makeuseof.com/tag/enable-spi-i2c-raspberry-pi/), (You need to restart your pi).

- Build and deploy Azure IoT Edge solution

We will use [Azure IoT Edge for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-edge) extension to build, push and deploy our solution.

First, build an push the solution by right click [deployment.template.json](/LoRaEngine/deployment.template.json) and select `Build and Push IoT Edge Solution` (look as alternative into [deployment.template.amd64.json](/LoRaEngine/deployment.template.amd64.json) for x64 based gateways)

![VSCode: Build and push edge solution](/pictures/CreateEdgeSolution.PNG)

After that you can push the solution to your IoT Edge device by right clicking on the device and select `Create Deployment for single device`

![VSCode: Deploy edge solution](/pictures/DeployEdge.PNG)

### Provision LoRa leaf device

The [sample code](/Arduino/TemperatureOtaaLora/TemperatureOtaaLora.ino) used in this example is based on [Seeeduino LoRaWAN](http://wiki.seeedstudio.com/Seeeduino_LoRAWAN/) with a [Grove - Temperature Sensor](http://wiki.seeedstudio.com/Grove-Temperature_Sensor_V1.2/). It sends every 30 seconds its current temperature reading and prints out a Cloud-2-Device message if one is transmitted in its receive window.

The sample has configured the following sample [device identifiers and credentials](https://www.thethingsnetwork.org/docs/lorawan/security.html):

- DevEUI: `47AAC86800430010`
- AppEUI: `BE7A0000000014E3`
- AppKey: `8AFE71A145B253E49C3031AD068277A3`

You will need your own identifiers when provisioning the device.

Look out for these code lines:

```arduino
lora.setId(NULL, "47AAC86800430010", "BE7A0000000014E3");
lora.setKey(NULL, NULL, "8AFE71A145B253E49C3031AD068277A3");
```

To provisioning a device in Azure IoT Hub with these identifiers and capable to [decode](/LoRaEngine/modules/LoRaWanNetworkSrvModule/LoRaWan.NetworkServer/LoraDecoders.cs) temperature payload into Json you have to create a device with:

Device Id: `47AAC86800430010` and Device Twin:

```json
"tags": {
  "AppEUI": "BE7A0000000014E3",
  "AppKey": "8AFE71A145B253E49C3031AD068277A3",
  "SensorDecoder": "DecoderTemperatureSensor"
}
```

![Create device in Azure IoT Hub](/pictures/CreateDevice.PNG)

![Set device twin in Azure IoT Hub](/pictures/DeviceTwin.PNG)

### Device to Cloud and Cloud to Device messaging in action

As soon as you start your device you should see the following:

- [DevAddr, AppSKey and NwkSKey](https://www.thethingsnetwork.org/docs/lorawan/security.html) are generated and stored in the Device Twin, e.g.:

```json
"tags": {
    "AppEUI": "BE7A0000000014E3",
    "AppKey": "8AFE71A145B253E49C3031AD068277A3",
    "SensorDecoder": "DecoderTemperatureSensor",
    "AppSKey": "5E8513F64D99A63753A5F0DBB9FB9F91",
    "NwkSKey": "C0EF4B9495BD4A4C32B42438CD52D4B8",
    "DevAddr": "025DEAAE",
    "DevNonce": "D9B6"
  }
```

- If you follow the logs of the network server module (e.g. `sudo iotedge logs LoRaWanNetworkSrvModule -f`) you can follow the LoRa device join:

```text
{"rxpk":[{"tmst":3831950603,"chan":2,"rfch":1,"freq":868.500000,"stat":1,"modu":"LORA","datr":"SF7BW125","codr":"4/5","lsnr":8.5,"rssi":-30,"size":23,"data":"AOMUAAAAAHq+EABDAGjIqkfZtkroyCc="}]}
Join Request Received
{"txpk":{"imme":false,"data":"IE633dznxvgA89ZTkH1jET0=","tmst":3836950603,"size":17,"freq":868.5,"rfch":0,"modu":"LORA","datr":"SF7BW125","codr":"4/5","powe":14,"ipol":true}}
Using edgeHub as local queue
Updating twins...
Join Accept sent
TX ACK RECEIVED
```

- Every 30 seconds the temperature is transmitted by the device, e.g.:

```json
{
  "time": null,
  "tmms": 0,
  "tmst": 4226472308,
  "freq": 868.5,
  "chan": 2,
  "rfch": 1,
  "stat": 1,
  "modu": "LORA",
  "datr": "SF12BW125",
  "codr": "4/5",
  "rssi": -33,
  "lsnr": 7.5,
  "size": 18,
  "data": {
    "temperature": 18.78
  },
  "EUI": "47AAC86800430010",
  "gatewayId": "berry2",
  "edgets": 1534253192857
}
```

Note: an easy way to follow messages send from the device is again with VSCode: right click on the device in the explorer -> `Start Monitoring D2C Message`.

This is how a complete transmission looks like:

![Complete transmission](/pictures/RoundtripTemp.PNG)

You can even test sending Cloud-2-Device message (e.g. by VSCode right click on the device in the explorer -> `Send C2D Message To Device`).

The Arduino example provided above will print the message on the console. Keep in mind that a [LoRaWAN Class A](https://www.thethingsnetwork.org/docs/lorawan/) device will only receive after a transmit, in our case every 30 seconds.
