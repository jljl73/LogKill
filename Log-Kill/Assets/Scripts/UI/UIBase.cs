using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LogKill.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public string ID;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public virtual void OnShow()
        {
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        public virtual void OnHide()
        {
        }

        public virtual void Initialize() { }
        public virtual async UniTask InitializeAsync() { await UniTask.Yield(); }
    }
}
