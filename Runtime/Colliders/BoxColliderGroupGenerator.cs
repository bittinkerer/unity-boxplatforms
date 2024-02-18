using Packages.com.esteny.platforms.Runtime.Colliders;
using Packages.Estenis.UnityExts_;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Generators
{
    public class BoxColliderGroupGenerator : MonoBehaviour
    {
        [SerializeField] private int _groupIndex;
        [SerializeField] private bool _drawGizmos;

        public BoxCollider ThisCollider =>
            this.gameObject.GetComponent<BoxCollider>();

        private void OnEnable()
        {
            if (ThisCollider == null)
            {
                Debug.LogError($"{this.name}.{nameof(BoxColliderRectangleGenerator)} needs a BoxCollider component to work with. Aborting.");
                return;
            }

            // If not the rightmost grouper then do nothing for now and return
            var rightNeighbor = GetNeighbor(ThisCollider.WorldRightPosition());
            if (rightNeighbor != null)
            {
                return;
            }

            // I am rightmost grouper, if no left neighbor then nothing to do so return
            var leftNeighbor = GetNeighbor(ThisCollider.WorldLeftPosition());
            if (leftNeighbor == null)
            {
                return;
            }

            // I am rightmost grouper and have left neighbor(s). Combine with left neighbor(s) boxcolliders.
            var leftNeighborCollider = leftNeighbor.GetLeftNeighborAndOwnBoxCollider(); 
            var resultCollider = CombineColliders(ThisCollider, leftNeighborCollider);
        }
        
        private BoxCollider GetLeftNeighborAndOwnBoxCollider()
        {
            // get left neighbor
            var leftNeighbor = GetNeighbor(ThisCollider.WorldLeftPosition());
            if (leftNeighbor == null)
            {
                return ThisCollider;
            }

            return CombineColliders(ThisCollider, leftNeighbor.GetLeftNeighborAndOwnBoxCollider());
        }

        private BoxColliderGroupGenerator GetNeighbor(Vector3 colliderPosition)
        {
            var collSize = ThisCollider.BoxSize();
            var boxSize = new Vector3(collSize.x, collSize.y/4, 1); // NOTE: y/4 so that vertical overlap must be bigger to group
            var grouper = Physics.OverlapBox(colliderPosition, boxSize)
                            .Where(coll => coll.transform != this.transform && coll.GetComponent<BoxColliderGroupGenerator>() != null)
                            .Select(coll => coll.GetComponent<BoxColliderGroupGenerator>())
                            .FirstOrDefault();
            
            return grouper;
        }

        private BoxCollider CombineColliders(BoxCollider right, BoxCollider left)
        {
            var resultCollider = right.gameObject.AddComponent<BoxCollider>();            
            var resultColliderWorldSize = new Vector3(right.bounds.size.x, right.bounds.size.y, right.bounds.size.z + left.bounds.size.z);
            var resultColliderWorldCenter = new Vector3(right.bounds.center.x, right.bounds.center.y, left.WorldLeftPosition().z + resultColliderWorldSize.z/2);
            resultCollider.center = right.transform.worldToLocalMatrix.MultiplyPoint(resultColliderWorldCenter);
            resultCollider.size = right.transform.worldToLocalMatrix.MultiplyVector(resultColliderWorldSize);

            Destroy(right);
            Destroy(left);

            return resultCollider;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos || ThisCollider == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0, 0, 0.65f);
            
            var collSize = ThisCollider.BoxSize();
            var boxSize = new Vector3(collSize.x, collSize.y/4, 1);
            Gizmos.DrawCube(ThisCollider.WorldLeftPosition(), boxSize);
            Gizmos.DrawCube(ThisCollider.WorldRightPosition(), boxSize);
        }

    }
}
