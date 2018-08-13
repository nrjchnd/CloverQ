# CloverQ

CloverQ is an ACD (Automatic Call Distributor) written in C# with VS2015. The main features are that can queue calls form multiple Asterisk servers, and can handle endpoints on different Asterisk server. The idea behind that is offload the registration and outbound traffic from the Astersiks working as queue servers. At the moment only works over chan_sip.

## Build status

||Windows|Linux (Mono)|
|:--:|:--:|:--:|
|Build|[![Build status](https://ci.appveyor.com/api/projects/status/github/cloversuite/cloverq?svg=true)](https://ci.appveyor.com/api/projects/status/github/cloversuite/)|[![Build Status](https://travis-ci.org/cloversuite/CloverQ.svg?branch=master)](https://travis-ci.org/cloversuite/CloverQ)

## Dependencies

* Akka.Net 1.2.0
* AsterNET.ARI 1.2.1
* Serilog 2.4.0

## Features

* Allows to spread the queues on multiple Asterisk servers
* Maintains the global order of a queued call
* Connect rtp of a queued call directly to the agent's sip phone
* Allows you to define queues and agents from a configuration file
* Manage agents independently of Asterisk
* Generates a queue log similar to the one generated by Asterisk
* Report time on hold during a call

## Setup

##### cloveq-conf.json file
* Define app names:
   * **StasisQueueAppName**: The name for the stasis queue app. Default: "cloverq"
   * **StasisLoginAppName**: The name for the stasis login app. Default: "cloverqLogin"
   * **StasisStateAppName**: The name for the stasis state app. Default: "cloverqState"
* Define asterisk servers
  * Each server must have the followin info: 
   ```  
    {
      "Ip": "192.168.1.21", //Ip of the asterisk server
      "Port": 8088, //Port of the ARI on asterisk server
      "User": "asterisk", //User name for ARI
      "Password": "123456" //Pass for ARI
    }
	```
   * **CallManagers**: Array of Asterisk hosts for queue server
   * **LoginProviders**: Array of Asterisk hosts for Login server. At the moment can define only one for login server
   * **StateProviders**: Array of Asterisk hosts for Login server. At the moment can define only one for login server and must be the same host for login server

##### datos-members.xml file
* Define the members (Agents) that can login into the system

##### datos-queues.xml file

* In this file you will define the queues and their parameters, and asign member ids defined in datos-members.xml to the conrreponding queues

* Id: Queue identifier that must be used in the dialplan when callind stasis queue app
* Media: Name of the music on hold to be played to the caller
* MediaType: At the moment only can be set to "MoH"
* Weight: Queue weight
* WrapupTime: Time that the queue system must wait before place a queued call to the queue member
* MemberStrategy: At the moment only can be set to "rrmemory" (round robin with memory)
* CallOrderStrategy: Defines the call ordering in the queue by his priority. Only can by set to "default"
* DTOQueueMember -> Priority: priority of the member in the queue

#### queue.log file
* This file is created quen the project starts, and log queue and member events
* TODO: queue log events list and description


### Dialplan Setup

On the servers that serve queues you must call stasis queue app, for example in extensions_custom.conf, here is an example:
[How to queue a call from dialplan](https://github.com/cloversuite/CloverQ/blob/master/Samples/Dialplan-QueuePBX.txt)

On the server that acts as Login and State provider you mas call stasis login app, for example in extensions_custom.conf, here is an example:
[How to handle agent login from dialplan](https://github.com/cloversuite/CloverQ/blob/master/Samples/Dialplan-LoginPBX.txt)

## How to compile the project
* Clone or download the project
* Open CloverQ.sln with VS2015 o VS2017
* Set CloverQServer project as StartUp project
* Press F5 to compile and run the project

