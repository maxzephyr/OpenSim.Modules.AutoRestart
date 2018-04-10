This is a simple little addon that restarts the region automatically.
You can set in the configuration in which time interval.

The addon takes care that nobody is in the region when the restart is executed.
Therefore it is no problem to restart the region every hour. Still, no one will ever notice because no one will ever get kicked out of it.

!!! Attention: The server shuts down completely. You need another tool that restarts the region automatically. !!!

Config:

[AutoRestart]
    Time = 30; //The server restarts every 30 minutes.
    