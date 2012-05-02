using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Listener : Handle, IListener
	{
		internal Listener(Loop loop, UvHandleType type)
			: base(loop, type)
		{
			DefaultBacklog = 128;
			listen_cb = listen_callback;
		}

		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_accept(IntPtr server, IntPtr client);

		public int DefaultBacklog { get; set; }

		Action<IntPtr, int> listen_cb;
		void listen_callback(IntPtr req, int status)
		{
			Stream stream = Create();
			uv_accept(req, stream.handle);
			OnListen(stream);
		}

		protected abstract Stream Create();

		protected event Action<Stream> OnListen;

		public void Listen(int backlog, Action<Stream> callback)
		{
			Ensure.ArgumentNotNull(callback, "callback");
			OnListen += callback;
			int r = uv_listen(handle, backlog, listen_cb);
			Ensure.Success(r, Loop);
		}

		public void Listen(Action<Stream> callback)
		{
			Listen(DefaultBacklog, callback);
		}
	}
}
