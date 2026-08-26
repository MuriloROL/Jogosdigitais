using UnityEngine;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    [ExecuteAlways]
    public sealed class UiVerticalGradient : BaseMeshEffect
    {
        [SerializeField] private Color top = Color.white;
        [SerializeField] private Color bottom = Color.white;

        public void Configure(Color topColor, Color bottomColor)
        {
            top = topColor;
            bottom = bottomColor;
            graphic?.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive() || vertexHelper.currentVertCount == 0)
            {
                return;
            }

            UIVertex vertex = default;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int index = 0; index < vertexHelper.currentVertCount; index++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, index);
                minY = Mathf.Min(minY, vertex.position.y);
                maxY = Mathf.Max(maxY, vertex.position.y);
            }

            float height = Mathf.Max(1f, maxY - minY);
            for (int index = 0; index < vertexHelper.currentVertCount; index++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, index);
                Color gradient = Color.Lerp(bottom, top, (vertex.position.y - minY) / height);
                vertex.color *= gradient;
                vertexHelper.SetUIVertex(vertex, index);
            }
        }
    }
}
