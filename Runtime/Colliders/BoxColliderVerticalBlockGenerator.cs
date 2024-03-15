using Packages.Estenis.UnityExts_;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.com.esteny.platforms.Runtime.Colliders
{
    public class BoxColliderVerticalBlockGenerator : MonoBehaviour
    {
        [SerializeField] private bool _drawGizmos;
        [SerializeField][Range(0, 1)] private float _removalBoxSize;

        private int _columnBlocks = -1;

        public bool IsTopOfBlock => GetNeighbor(ThisCollider.WorldTopPosition()) == null;
        public int ColumnBlocks => 
            _columnBlocks >= 0 
            ? _columnBlocks 
            : (_columnBlocks = GetColumnBlocks(this.GetComponent<BoxCollider>()));

        public BoxCollider ThisCollider =>
            this.gameObject.GetComponent<BoxCollider>();

        private void OnEnable()
        {
            if(ThisCollider == null)
            {
                Debug.LogError($"{this.name}.{nameof(BoxColliderVerticalBlockGenerator)} needs a BoxCollider component to work with. Aborting.");
                return;
            }

            // If not the topmost grouper then do nothing for now and return
            if (!IsTopOfBlock)
            {
                return;
            }

            // 
            var rightNeighbor = GetNeighbor(ThisCollider.WorldRightPosition());
            if (rightNeighbor != null && (rightNeighbor.IsTopOfBlock && rightNeighbor.ColumnBlocks == this.ColumnBlocks))
            {
                return;
            }

            // I am right-most top of block collider, consolidate myself and column-blocks to left matching me



            var (left, right) = GetOwnAndLeftHorizontalBounds();
            float horizontalSize = right - left;
            var (top, bottom) = GetOwnAndBottomVerticalBounds();
            float verticalSize = top - bottom;

            Vector3 center = new(
                    ThisCollider.bounds.center.x,
                    ThisCollider.WorldTopPosition().y - verticalSize / 2,
                    ThisCollider.WorldRightPosition().z - horizontalSize / 2
                );
            Vector3 size = new(
                ThisCollider.size.x,
                verticalSize,
                horizontalSize
                );
            DestroyColliders(
                center,
                size * _removalBoxSize);
            CreateCollider(this.transform, center, size);
        }

        private int GetColumnBlocks(BoxCollider col)
        {
            if(col == null)
            {
                return 0;
            }

            return 1 + GetColumnBlocks(GetNeighbor(col.WorldBottomPosition())?.GetComponent<BoxCollider>());
        }


        private (float left, float right) GetOwnAndLeftHorizontalBounds()
        {
            (float left, float right) result;
            var leftNeighbor = GetNeighbor(ThisCollider.WorldLeftPosition());
            if (leftNeighbor == null)
            {
                result = (ThisCollider.WorldLeftPosition().z, ThisCollider.WorldRightPosition().z);
            }
            else
            {
                result =
                    (
                        leftNeighbor.GetOwnAndLeftHorizontalBounds().left,
                        ThisCollider.WorldRightPosition().z
                    );
            }
            Destroy(ThisCollider);
            return result;
        }

        private (float top, float bottom) GetOwnAndBottomVerticalBounds()
        {
            (float top, float bottom) result;
            var bottomNeighbor = GetNeighbor(ThisCollider.WorldBottomPosition());
            if (bottomNeighbor == null)
            {
                result = (ThisCollider.WorldTopPosition().y, ThisCollider.WorldBottomPosition().y);
            }
            else
            {
                result =
                    (
                        ThisCollider.WorldTopPosition().y,
                        bottomNeighbor.GetOwnAndBottomVerticalBounds().bottom
                    );
            }
            Destroy(ThisCollider);
            return result;
        }

        private float GetLeftNeighborAndOwnZSize()
        {
            float result;
            var leftNeighbor = GetNeighbor(ThisCollider.WorldLeftPosition());
            if (leftNeighbor == null)
            {
                result = ThisCollider.size.z;
            }
            else
            {
                result = ThisCollider.size.z + leftNeighbor.GetLeftNeighborAndOwnZSize();
            }
            Destroy(ThisCollider);
            return result;
        }

        private float GetBottomNeighborAndOwnYSize()
        {
            float result;
            var bottomNeighbor = GetNeighbor(ThisCollider.WorldBottomPosition());
            if (bottomNeighbor == null)
            {
                result = ThisCollider.size.y;
            }
            else
            {
                result = ThisCollider.size.y + bottomNeighbor.GetBottomNeighborAndOwnYSize();
            }
            Destroy(ThisCollider);
            return result;
        }

        private BoxColliderVerticalBlockGenerator GetNeighbor(Vector3 colliderPosition)
        {
            var collSize = ThisCollider.BoxSize();
            var boxSize = new Vector3(collSize.x, collSize.y / 4, collSize.z / 4); // NOTE: y/4 so that vertical overlap must be bigger to group
            var grouper = Physics.OverlapBox(colliderPosition, boxSize)
                            .Where(coll => coll.transform != this.transform && coll.GetComponent<BoxColliderVerticalBlockGenerator>() != null)
                            .Select(coll => coll.GetComponent<BoxColliderVerticalBlockGenerator>())
                            .FirstOrDefault();

            return grouper;
        }

        private void DestroyColliders(Vector3 center, Vector3 size)
        {
            var colls = Physics.OverlapBox(center, size);
            foreach (var coll in colls)
            {
                Destroy(coll);
            }

        }

        private BoxCollider CreateCollider(Transform topRightTransform, Vector3 center, Vector3 size)
        {
            var resultCollider = topRightTransform.gameObject.AddComponent<BoxCollider>();
            
            resultCollider.center = topRightTransform.worldToLocalMatrix.MultiplyPoint(center);
            resultCollider.size = topRightTransform.worldToLocalMatrix.MultiplyVector(size);
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
            var boxSize = new Vector3(collSize.x, collSize.y / 4, collSize.z/4);
            Gizmos.DrawCube(ThisCollider.WorldLeftPosition(), boxSize);
            Gizmos.DrawCube(ThisCollider.WorldRightPosition(), boxSize);
            Gizmos.DrawCube(ThisCollider.WorldTopPosition(), boxSize);
            Gizmos.DrawCube(ThisCollider.WorldBottomPosition(), boxSize);
        }
    }
}
