using UnityEngine;

namespace BitHeroesInternal
{
    internal class BHInternal : MonoBehaviour
    {
        private InternalWindow window;

        public void Start()
        {
            window = gameObject.AddComponent<InternalWindow>();
        }

        public void Update()
        {
            // Press F8 to toggle the internal window
            if (Input.GetKeyDown(KeyCode.F8))
            {
                window.Toggle();
            }
        }
    }
}
