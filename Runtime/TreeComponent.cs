﻿#if FIXED_POINT_MATH
using Saro.FPMath;
using Single = Saro.FPMath.sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using Saro.Entities;
using UnityEngine;

namespace Saro.BT
{
    [System.Obsolete]
    public class TreeComponent : MonoBehaviour
    {
        /// <summary>
        /// runtime tree
        /// </summary>
        public BehaviorTree RuntimeTree => m_RuntimeTree;

        public BehaviorTree TreeAsset { get => m_TreeAsset; set => m_TreeAsset = value; }

        public bool TickManual { get => m_TickManual; set => m_TickManual = value; }

        //[SerializeField]
        //private TextAsset m_TreeTextAsset; // only for runtime

        [SerializeField]
        private BehaviorTree m_TreeAsset;

        [SerializeField]
        private bool m_TickManual;

        private BehaviorTree m_RuntimeTree;

        public void Init(EcsEntity actor)
        {
            m_RuntimeTree = BehaviorTree.CreateRuntimeTree(TreeAsset, actor);

            this.enabled = !m_TickManual;
        }

        public void Run()
        {
            if (m_RuntimeTree != null)
                m_RuntimeTree.BeginTraversal();
        }

        public void Tick()
        {
            if (m_RuntimeTree != null)
                m_RuntimeTree.Tick((Single)Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Tick();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (m_RuntimeTree != null)
                Destroy(m_RuntimeTree);
#endif
        }
    }
}