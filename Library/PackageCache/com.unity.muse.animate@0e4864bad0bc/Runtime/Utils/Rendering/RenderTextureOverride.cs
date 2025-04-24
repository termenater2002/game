using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
	struct RenderTextureOverride : IDisposable
	{
		public RenderTextureOverride(RenderTexture renderTexture)
		{
			m_BackupRT = RenderTexture.active;
			RenderTexture.active = renderTexture;
		}

		private RenderTexture m_BackupRT;

		public void Dispose()
		{
			RenderTexture.active = m_BackupRT;
		}
	}
}
