using System;
using StyleTrainer.Backend;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class ImageArtifact : Artifact<Texture2D, ImageArtifact>
    {
        StyleTrainerImageArtifactCache m_Cache;
        public const string k_PlaceHolderGUID = "StyleTrainer-PlaceHolder-Cache-GUID";
        public const string k_ForbiddenTextureGUID = "StyleTrainer-Forbidden-Texture-GUID";
        public const string k_ErrorTextureGUID = "StyleTrainer-Error-Texture-GUID";
        bool m_NoPlaceholder = false;
        public ImageArtifact(EState state)
            : base(state)
        {
            m_Cache = new StyleTrainerImageArtifactCache(guid, 0);
        }

        Texture2D SetPlaceHolderTexture()
        {
            if (m_Cache?.Guid != k_PlaceHolderGUID)
            {
                m_Cache?.Dispose();
                m_Cache = new StyleTrainerImageArtifactCache(k_PlaceHolderGUID, 0);
            }

            var texture = m_Cache.GetTexture2D();
            if (texture == null)
            {
                texture = Utilities.placeHolderTexture;
                if (!texture.isReadable)
                    texture = BackendUtilities.CreateTemporaryDuplicate(texture, texture.width, texture.height);
                var rawData = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);
                m_Cache.WriteToCache(rawData);
                texture = m_Cache.GetTexture2D();
            }

            return texture;
        }

        public bool disablePlaceHolder
        {
            set => m_NoPlaceholder = value;
            get => m_NoPlaceholder;
        }

        public (bool cached, Texture2D texture) GetLoaded()
        {
            if (Utilities.ValidStringGUID(guid) && m_Cache != null && m_Cache.Guid == guid && m_Cache.IsLoaded())
            {
                return (true, m_Cache.GetTexture2D());
            }

            return (false, null);
        }

        public override void GetArtifact(Action<Texture2D> onDoneCallback, bool useCache)
        {
            if (!Utilities.ValidStringGUID(guid))
            {
                if (m_Cache?.Guid != k_PlaceHolderGUID)
                {
                    DisposeCache();
                    m_Cache = new StyleTrainerImageArtifactCache(k_PlaceHolderGUID, 0);
                }
                if(!disablePlaceHolder)
                    onDoneCallback?.Invoke(SetPlaceHolderTexture());
            }
            else if (Utilities.IsTempGuid(guid))
            {
                if (m_Cache?.Guid != guid)
                {
                    DisposeCache();
                    m_Cache = new StyleTrainerImageArtifactCache(guid, 0);
                }
                var texture = m_Cache.GetTexture2D();
                if (texture == null)
                {
                    StyleTrainerDebug.Log($"Image artifact is a temp guid {guid} but cache is in cache. Was this set via SetTexture?");
                    texture = SetPlaceHolderTexture();
                }

                onDoneCallback?.Invoke(texture);
            }
            else
            {
                if (m_Cache?.Guid != guid)
                {
                    DisposeCache();
                    m_Cache = new StyleTrainerImageArtifactCache(guid, 0);
                }

                var texture = m_Cache.GetTexture2D();
                if (useCache && (state == EState.Loaded || state == EState.New) && texture != null)
                {
                    state = EState.Loaded;
                    onDoneCallback?.Invoke(texture);
                }
                else
                {
                    OnArtifactLoaded += onDoneCallback;
                    state = EState.Loading;
                    var getImageRequest = new GetImageRequest
                    {
                        guid = guid
                    };
                    var getImageRestCall = new GetImageFromURLRestCall(ServerConfig.serverConfig, getImageRequest);
                    getImageRestCall.RegisterOnSuccess(OnGetImageFromURLSuccess);
                    getImageRestCall.RegisterOnFailure(OnGetImageFromURLFailure);
                    getImageRestCall.SendRequest();
                }
            }
        }

        public void DeleteCache()
        {
            // Don't delete cache if it's new or if it's a placeholder or error texture
            if (state == EState.New && m_Cache?.Guid != k_PlaceHolderGUID && m_Cache?.Guid != k_ErrorTextureGUID)
                m_Cache?.DeleteCache();
        }

        public override void OnDispose()
        {
            DisposeCache();
            base.OnDispose();
        }

        bool IsSharedTexture()
        {
            return m_Cache?.Guid == k_PlaceHolderGUID ||
                m_Cache?.Guid == k_ErrorTextureGUID ||
                m_Cache?.Guid == k_ForbiddenTextureGUID;
        }
        void DisposeCache()
        {
            if ((state == EState.New || state == EState.Loaded) && !IsSharedTexture())
                m_Cache?.Dispose();
        }

        void OnGetImageFromURLFailure(GetImageFromURLRestCall obj)
        {
            if (obj.restCallState == QuarkRestCall.EState.Forbidden)
            {
                LoadImageForbidden();
            }
            else if (obj.retriesFailed)
                LoadImageFailed();
        }

        void LoadImageForbidden()
        {
            state = EState.Error;
            if (m_Cache?.Guid != k_ForbiddenTextureGUID)
            {
                DisposeCache();
                m_Cache = new StyleTrainerImageArtifactCache(k_ForbiddenTextureGUID, 0);
            }

            var texture = m_Cache.GetTexture2D();
            if (texture == null)
            {
                texture = Utilities.forbiddenTexture;
                if (!texture.isReadable)
                    texture = BackendUtilities.CreateTemporaryDuplicate(texture, texture.width, texture.height);
                var rawData = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);
                m_Cache.WriteToCache(rawData);
            }

            texture = m_Cache.GetTexture2D();
            ArtifactLoaded(texture);
        }

        void OnGetImageFromURLSuccess(GetImageFromURLRestCall arg1, byte[] arg2)
        {
            LoadImageSuccess(arg2);
        }

        void LoadImageFailed()
        {
            state = EState.Error;
            if (m_Cache?.Guid != k_ErrorTextureGUID)
            {
                DisposeCache();
                m_Cache = new StyleTrainerImageArtifactCache(k_ErrorTextureGUID, 0);
            }

            var texture = m_Cache.GetTexture2D();
            if (texture == null)
            {
                texture = Utilities.errorTexture;
                if (!texture.isReadable)
                    texture = BackendUtilities.CreateTemporaryDuplicate(texture, texture.width, texture.height);
                var rawData = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);
                m_Cache.WriteToCache(rawData);
            }

            texture = m_Cache.GetTexture2D();
            ArtifactLoaded(texture);
        }

        void LoadImageSuccess(byte[] bytes)
        {
            state = EState.Loaded;
            SetTexture(bytes);
            ArtifactLoaded(m_Cache.GetTexture2D());
        }

        protected override void GUIDChanged()
        {
            if (m_Cache.Guid != k_PlaceHolderGUID && m_Cache.Guid != k_ErrorTextureGUID)
                m_Cache.ChangeCacheKey(guid);
            else
                m_Cache.ChangeGUID(guid);
            base.GUIDChanged();

            //DataChanged(this);
        }

        /// <summary>
        /// Force set the texture of the artifact. This is usually used when we are storing local data
        /// before it is uploaded to the server
        /// </summary>
        /// <param name="texture"></param>
        public void SetTexture(byte[] texture)
        {
            if (!Utilities.ValidStringGUID(guid) && !Utilities.IsTempGuid(guid))
            {
                guid = Utilities.CreateTempGuid();
                m_Cache.Guid = guid;
            }

            m_Cache.WriteToCache(texture);
            DataChanged(this);
        }

        public string Base64Image()
        {
            var data = m_Cache.GetRawDataDirect();
            return data != null ? Convert.ToBase64String(data) : "";
        }

        public string Base64ImageNoAlpha()
        {
            // var t = new Texture2D(2, 2);
            //
            // t.LoadImage(m_TextureData);
            // t = BackendUtilities.CreateTemporaryDuplicate(t, t.width, t.height, TextureFormat.RGB24);
            // return Convert.ToBase64String(t.EncodeToPNG());

            // we have already convert them to non alpha before adding it into the training set
            return Base64Image();
        }

        public byte[] GetRawData()
        {
            return m_Cache.GetRawDataDirect();
        }
    }
}