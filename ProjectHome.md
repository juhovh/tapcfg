# Tapcfg Virtual Networking #

## Short Description ##

A library and related utilities to make it easier to configure the TAP device driver regardless of the platform being run on. This should make virtual networking on **Linux/MacOSX/FreeBSD/NetBSD/Solaris/Windows (XP/Vista/7)** much easier for application developers.

TAP is a virtual networking device which allows userland applications to receive all packets sent to it including the ethernet headers. The userland applications can also send raw packets to the TAP device and they will be forwarded to the kernel like they would be coming from a real network device.

## Supported Features ##

Currently the library supports creating a new TAP device for use and the following extra features for it:

  * Autodetection of available TAP devices if the name is not known
  * Getting and setting the MAC address of the device (setting on Windows has to be done from driver GUI)
  * Setting the device to up and down status (on Windows connect and disconnect the wire to the device)
  * Getting and setting the default MTU of the device (setting not available on Windows)
  * Setting the IPv4 address and netmask of the device on all platforms
  * Initialization of IPv6 stack and setting the device into receiving router advertisement packets on all platforms that specifically require it

## Requirements ##

On Linux, FreeBSD and NetBSD the application will work with drivers bundled with the kernel. On other systems installation of 3rd party drivers is required, but they are all available in source and binary form with an Open Source license. The source code of 3rd party drivers is also bundled with the tapcfg source tree, but they are not modified or updated in any way. Tested versions of the 3rd party drivers will be released with future binary releases of tapcfg.

## Possible Uses ##

Easy creation of cross-platform tunnels, virtual networks or anything requiring a virtual network interface device. There are examples provided that implement a tunnel over TCP/IP and should compile on all supported systems without modifications. They can also be used as basis for further implementations, since most of the cross-platform glue is already provided.

There is also P/Invoke .Net bindings included that support automatic loading of 32-bit or 64-bit library depending on the host system, and support all the features of the library conveniently from a managed language. This should make the barrier of developing virtual networking applications even lower.

## License ##

All the code is licensed under the fairly liberal GNU Lesser General Public License (LGPL) version 2.1 or later. To make it short, modifications to the library need to be released while distributing and users need to be able to replace the library with a modified version at will. The full license and more information can be found from the [GNU website](http://www.gnu.org/copyleft/lesser.html).