using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UIAvatarPresenter : MonoBehaviour
    {
        private string _imageUrl = "";

        public void Init(string imageUrl)
        {
            if (_imageUrl != imageUrl)
            {
                _imageUrl = imageUrl;
                StartCoroutine(DownloadImage(imageUrl));
            }
        }

        private IEnumerator DownloadImage(string mediaUrl)
        {
            var request = UnityWebRequestTexture.GetTexture(mediaUrl);
            yield return request.SendWebRequest();
            if (request.responseCode != 200)
                Debug.Log("Load image failed");
            else
                GetComponent<RawImage>().texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}