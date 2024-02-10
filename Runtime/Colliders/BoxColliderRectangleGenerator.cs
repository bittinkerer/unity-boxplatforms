using Packages.Estenis.UnityExts_;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.com.esteny.platforms.Runtime.Colliders
{
    public class BoxColliderRectangleGenerator : MonoBehaviour
    {
        [SerializeField] private bool _drawGizmos;

        public BoxCollider ThisCollider =>
            this.gameObject.GetComponent<BoxCollider>();

        private void OnEnable()
        {
            // If not the rightmost grouper then do nothing for now and return
            var rightNeighbor = GetNeighbor(ThisCollider.WorldRightPosition());
            if (rightNeighbor != null)
            {
                return;
            }

            // If not the topmost grouper then do nothing for now and return
            var topNeighbor = GetNeighbor(ThisCollider.WorldTopPosition());
            if (topNeighbor != null)
            {
                return;
            }

            float horizontalSize = GetLeftNeighborAndOwnZSize();
            float verticalSize = GetBottomNeighborAndOwnYSize();

            var resultCollider = CreateCollider(ThisCollider, new Vector3(ThisCollider.size.x, verticalSize, horizontalSize));
        }

        private float GetLeftNeighborAndOwnZSize()
        {
            var leftNeighbor = GetNeighbor(ThisCollider.WorldLeftPosition());
            if (leftNeighbor == null)
            {
                return ThisCollider.size.z;
            }

            return ThisCollider.size.z + leftNeighbor.GetLeftNeighborAndOwnZSize();
        }

        private float GetBottomNeighborAndOwnYSize()
        {
            var bottomNeighbor = GetNeighbor(ThisCollider.WorldBottomPosition());
            if (bottomNeighbor == null)
            {
                return ThisCollider.size.y;
            }

            return ThisCollider.size.y + bottomNeighbor.GetBottomNeighborAndOwnYSize();
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

        private BoxColliderRectangleGenerator GetNeighbor(Vector3 colliderPosition)
        {
            var collSize = ThisCollider.BoxSize();
            var boxSize = new Vector3(collSize.x, collSize.y / 4, 1); // NOTE: y/4 so that vertical overlap must be bigger to group
            var grouper = Physics.OverlapBox(colliderPosition, boxSize)
                            .Where(coll => coll.transform != this.transform && coll.GetComponent<BoxColliderRectangleGenerator>() != null)
                            .Select(coll => coll.GetComponent<BoxColliderRectangleGenerator>())
                            .FirstOrDefault();

            return grouper;
        }

        private BoxCollider CombineColliders(BoxCollider right, BoxCollider left)
        {
            var resultCollider = right.gameObject.AddComponent<BoxCollider>();
            var resultColliderWorldSize = 
                new Vector3(right.bounds.size.x, right.bounds.size.y, right.bounds.size.z + left.bounds.size.z);
            var resultColliderWorldCenter = 
                new Vector3(right.bounds.center.x, right.bounds.center.y, left.WorldLeftPosition().z + resultColliderWorldSize.z / 2);
            resultCollider.center = right.transform.worldToLocalMatrix.MultiplyPoint(resultColliderWorldCenter);
            resultCollider.size = right.transform.worldToLocalMatrix.MultiplyVector(resultColliderWorldSize);

            Destroy(right);
            Destroy(left);

            return resultCollider;
        }

        private BoxCollider CreateCollider(BoxCollider topRight, Vector3 size)
        {
            var resultCollider = topRight.gameObject.AddComponent<BoxCollider>();
            var resultColliderWorldSize = 
                new Vector3(topRight.size.x, size.y, size.z);
            var resultColliderWorldCenter = 
                new Vector3(
                    topRight.bounds.center.x, 
                    topRight.WorldTopPosition().y - size.y/2, 
                    topRight.WorldRightPosition().z - size.z/2);
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
            var boxSize = new Vector3(collSize.x, collSize.y / 4, 1);
            Gizmos.DrawCube(ThisCollider.WorldLeftPosition(), boxSize);
            Gizmos.DrawCube(ThisCollider.WorldRightPosition(), boxSize);
        }
    }
}
