**SdMessenger** is a client-server chat application written in C#. Unlike typical examples, chat server is single-threaded and uses synchronous sockets. However, its design makes possible to distribute load over multiple threads.

**Features:**
* Private messages (the only mode so far)
* Client-server traffic encryption: AES/Blowfish with RSA key exchange
* File transfer
  * Should handle really large files
  * Individual sessions can be suspended/resumed
* Pure console chat server
  * Is administered via command line
  * Password-protected user accounts with different access rights
* Works over the Internet
