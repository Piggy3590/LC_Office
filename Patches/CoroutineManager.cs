using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCKorean.Patches
{
    public static class CoroutineManager
    {
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return instance.StartCoroutine(routine);
        }

        private static MonoBehaviour instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("CoroutineManager").AddComponent<CoroutineManagerBehaviour>();
                }
                return _instance;
            }
        }

        private static MonoBehaviour _instance;

        internal class CoroutineManagerBehaviour : MonoBehaviour { }
    }
}