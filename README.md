# Description
Code snippet from bigger plugin to demonstrate my coding capabilities.

Socket dependency is a dependency to integrate different socket used in the game. It will hold different socket and send message into game for one direct channel. So game didn't need to know the low level of socket that we use. Currently only support for Websocket and WebRTC.

Socket use priority is first WebRTC and then Websocket.

# How this works
When host start websocket, it will connect to server and ready to receive message. Client than connect to server and connect to host. Websocket will open for host and client.

After that client will create WebRTC offer and send that message into host with Websocket. Client also send Ice Candidate to host.

Host will receive offer from client and set remote description to client. Host will create answer and send it back to client. Host also send Ice Candidate to client. After that WebRTC should open if the device support it. 

# Message
SocketDependency will take notice if Websocket or WebRTC is receiving message. When message arrive SocketDependency will notify host for that message. For host, it will only care for message arrive not which socket it arrive. 

When host want to send message it will call SocketDependency. SocketDependency than try to send that message with WebRTC. If WebRTC is not open, it will send with Websocket.

Message type is in JSON. I'm using Newtonsoft Plugin to read and write JSON. I optimize read and write JSON using basic JsonTextReader and JsonTextWriter
