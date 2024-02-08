using Assets.Scripts.Generators;
using Packages.Estenis.EventActions_;
using UnityEditor;
using UnityEngine;
using Packages.Estenis.UnityExts_;
using System.Linq;

namespace Packages.com.esteny.platforms.Runtime.Actions
{
    public class BoxColliderGroundGenerateAction : BaseGameObjectAction
    {
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private float _height;
        [SerializeField] private string _groundLayer;

        public BoxCollider ThisCollider =>
            this.gameObject.GetComponent<BoxCollider>();

        protected override void Action(object data)
        {
            if(ThisCollider == null)
            {
                Debug.LogError($"{this.name}.{nameof(BoxColliderGroundGenerateAction)} needs a valid {nameof(BoxCollider)} on the gameobject. Exiting.");
                return;
            }

            GenerateGroundCollider(ThisCollider);
        }

        private void GenerateGroundCollider(BoxCollider baseCollider)
        {
            var groundGO = new GameObject("__GroundLayer")
            {
                layer = LayerMask.NameToLayer(_groundLayer)
            };
            var groundCollider = groundGO.AddComponent<BoxCollider>();

            // calc size
            var sizeInWorldCoords = new Vector3(baseCollider.bounds.size.x, _height, baseCollider.bounds.size.z);
            var sizeInLocalCoords = baseCollider.transform.worldToLocalMatrix.MultiplyVector(sizeInWorldCoords);
            groundCollider.size = sizeInLocalCoords;

            // calc center
            var centerInWorldCoords = new Vector3(
                baseCollider.bounds.center.x,
                (baseCollider.bounds.center.y + baseCollider.bounds.size.y / 2) - (_height / 2),
                baseCollider.bounds.center.z
                );
            var centerInLocalCoords = baseCollider.transform.worldToLocalMatrix.MultiplyPoint(centerInWorldCoords);
            groundCollider.center = centerInLocalCoords;

        }

        
        private void OnDrawGizmos()
        {
            if (!_drawGizmos || ThisCollider == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0, 0, 0.65f);

            // calc size
            var sizeInWorldCoords = new Vector3(ThisCollider.bounds.size.x, _height, ThisCollider.bounds.size.z);
            var sizeInLocalCoords = ThisCollider.transform.worldToLocalMatrix.MultiplyVector(sizeInWorldCoords);

            // calc center
            var centerInWorldCoords = new Vector3(
                ThisCollider.bounds.center.x,
                (ThisCollider.bounds.center.y + ThisCollider.bounds.size.y / 2) - (_height / 2),
                ThisCollider.bounds.center.z
                );
            var centerInLocalCoords = ThisCollider.transform.worldToLocalMatrix.MultiplyPoint(centerInWorldCoords);

            Gizmos.DrawCube(centerInWorldCoords - sizeInWorldCoords / 2, sizeInWorldCoords);
        }
    }
}
