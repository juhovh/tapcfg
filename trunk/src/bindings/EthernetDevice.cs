/**
 *  tapcfg - A cross-platform configuration utility for TAP driver
 *  Copyright (C) 2008  Juho Vähä-Herttua
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace TAP {
	public delegate void EthernetLogCallback(
		[MarshalAs(UnmanagedType.CustomMarshaler,
			   MarshalTypeRef = typeof(UTF8Marshaler))]
		string msg);

	public class EthernetDevice : IDisposable {
		/* Default MTU 1500 in all systems */
		private int _MTU = 1500;

		private IntPtr _handle;
		private bool _disposed = false;

		private static EthernetLogCallback _logger;
		public static EthernetLogCallback LogCallback {
			set {
				_logger = value;
				taplog_set_callback(_logger);
			}
		}

		private static void defaultCallback(string msg) {
			Console.WriteLine(msg);
		}

		public EthernetDevice() {
			if (_logger == null) {
				_logger = new EthernetLogCallback(defaultCallback);
			}

			_handle = tapcfg_init();
			if (_handle == IntPtr.Zero) {
				throw new Exception("Error initializing the tapcfg library");
			}
		}

		public void Start() {
			Start(null);
		}

		public void Start(string deviceName) {
			int ret = tapcfg_start(_handle, deviceName);
			if (ret < 0) {
				throw new Exception("Error starting the TAP device");
			}
		}

		public bool WaitReadable(int msec) {
			int ret = tapcfg_wait_readable(_handle, msec);
			if (ret != 0) {
				return true;
			} else {
				return false;
			}
		}

		public EthernetFrame Read() {
			/* Maximum buffer is MTU plus 22 byte maximum header size */
			byte[] buffer = new byte[_MTU + 22];

			int ret = tapcfg_read(_handle, buffer, buffer.Length);
			if (ret < 0) {
				throw new IOException("Error reading Ethernet frame");
			} else if (ret == 0) {
				throw new EndOfStreamException("Unexpected EOF");
			}

			return new EthernetFrame(buffer);
		}

		public bool WaitWritable(int msec) {
			int ret = tapcfg_wait_writable(_handle, msec);
			if (ret != 0) {
				return true;
			} else {
				return false;
			}
		}

		public void Write(EthernetFrame frame) {
			byte[] buffer = frame.Data;

			int ret = tapcfg_write(_handle, buffer, buffer.Length);
			if (ret < 0) {
				throw new IOException("Error writing Ethernet frame");
			} else if (ret != buffer.Length) {
				/* This shouldn't be possible, writes are blocking */
				throw new IOException("Incomplete write writing Ethernet frame");
			}
		}

		public bool Enabled {
			get {
				int ret = tapcfg_iface_get_status(_handle);
				if (ret != 0) {
					return true;
				} else {
					return false;
				}
			}
			set {
				int ret;

				if (value) {
					ret = tapcfg_iface_change_status(_handle, 1);
				} else {
					ret = tapcfg_iface_change_status(_handle, 0);
				}

				if (ret < 0) {
					throw new Exception("Error changing TAP interface status");
				}
			}
		}

		public string DeviceName {
			get { return tapcfg_get_ifname(_handle); }
		}

		public int MTU {
			get {
				return _MTU;
			}
			set {
				int ret = tapcfg_iface_set_mtu(_handle, value);
				if (ret >= 0) {
					_MTU = value;
				}
			}
		}

		public void SetAddress(IPAddress address, byte netbits) {
			int ret;

			if (address.AddressFamily == AddressFamily.InterNetwork) {
				ret = tapcfg_iface_set_ipv4(_handle, address.ToString(), netbits);
			} else {
				return;
			}

			if (ret < 0) {
				throw new Exception("Error setting IP address: " + address.ToString());
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!_disposed) {
				if (disposing) {
					// Managed resources can be _disposed here
				}

				tapcfg_stop(_handle);
				tapcfg_destroy(_handle);
				_handle = IntPtr.Zero;

				_disposed = true;
			}
		}

		[DllImport("tapcfg")]
		private static extern void taplog_set_callback(EthernetLogCallback cb);

		[DllImport("tapcfg")]
		private static extern IntPtr tapcfg_init();
		[DllImport("tapcfg")]
		private static extern void tapcfg_destroy(IntPtr tapcfg);

		[DllImport("tapcfg")]
		private static extern int tapcfg_start(IntPtr tapcfg,
			[MarshalAs(UnmanagedType.CustomMarshaler,
			           MarshalTypeRef = typeof(UTF8Marshaler))]
			string ifname);
		[DllImport("tapcfg")]
		private static extern void tapcfg_stop(IntPtr tapcfg);

		[DllImport("tapcfg")]
		private static extern int tapcfg_wait_readable(IntPtr tapcfg, int msec);
		[DllImport("tapcfg")]
		private static extern int tapcfg_wait_writable(IntPtr tapcfg, int msec);

		[DllImport("tapcfg")]
		private static extern int tapcfg_read(IntPtr tapcfg, byte[] buf, int count);
		[DllImport("tapcfg")]
		private static extern int tapcfg_write(IntPtr tapcfg, byte[] buf, int count);

		[DllImport("tapcfg")]
		[return : MarshalAs(UnmanagedType.CustomMarshaler,
		                    MarshalTypeRef = typeof(UTF8Marshaler))]
		private static extern string tapcfg_get_ifname(IntPtr tapcfg);

		[DllImport("tapcfg")]
		private static extern int tapcfg_iface_get_status(IntPtr tapcfg);
		[DllImport("tapcfg")]
		private static extern int tapcfg_iface_change_status(IntPtr tapcfg, int enabled);
		[DllImport("tapcfg")]
		private static extern int tapcfg_iface_set_mtu(IntPtr tapcfg, int mtu);
		[DllImport("tapcfg")]
		private static extern int tapcfg_iface_set_ipv4(IntPtr tapcfg, string addr, byte netbits);
	}
}