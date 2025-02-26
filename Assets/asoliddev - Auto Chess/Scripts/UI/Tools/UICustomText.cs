using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game
{
    /// 文本控件，支持图片
    [AddComponentMenu("UI/UICustomText", 1)]
    [RequireComponent(typeof(Shadow), typeof(Outline))]
    public class UICustomText : Text, IPointerClickHandler
    {
        // 正在被使用的图片
        private readonly List<Image> m_UsedImages = new List<Image>();

        // 图片的第一个顶点的索引
        private readonly List<int> m_ImageStartVertexIndices = new List<int>();

        // 正则取出所需要的属性
        private static readonly Regex s_ImageRegex =
            new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

        static readonly Regex s_ImageEmpty = new Regex(@"a", RegexOptions.Singleline);

        static readonly Regex s_ImageRegex1 =
            new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) />", RegexOptions.Singleline);

        static readonly Regex s_ImageRegex2 =
            new Regex(@"<quad name=(.+?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

        static readonly Regex s_ImageRegex3 = new Regex(@"<quad name=(.+?) />", RegexOptions.Singleline);

        // 加载精灵图片方法
        public static Func<Image, string, Sprite> OnLoadSprite;

        private UIVertex[] m_TempVerts = new UIVertex[4];

        //为了不破坏原有规则 缓存原始值，外部访问获取 返回该值
        private string text_cache = string.Empty;

        public bool useAutoArabic = true;

        private bool m_ImgDirty = false;
        private int m_ShowImgCount = 0;
        public bool isUseMaxWidth;
        public float maxWidth;
        private static readonly StringBuilder TextBuilder = new StringBuilder();

        protected bool ShouldFlip = false;

        public void SetFlip(bool flip)
        {
            ShouldFlip = flip;
        }
        
        private float _shadowPerFontSize = 0.05f;
        private float _outlinePerFontSize = 0.05f;
        private int _fontSize = 14;
        public override string text
        {
            get => text_cache;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    if (String.IsNullOrEmpty(text_cache) && String.IsNullOrEmpty(m_Text))
                        return;
                    SetVerticesDirty();
                }
                else if (text_cache != value || m_Text != value)
                {
                    text_cache = m_Text = value;
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }
        
        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            GenerateQuadImage();
            GetOutputText();
            CheckMaxWidth();
            CheckOutlineAndShadow();
            //Debug.Log("SetVerticesDirty");
        }

        private void CheckMaxWidth()
        {
            if (isUseMaxWidth)
            {
                if (preferredWidth > maxWidth)
                {
                    gameObject.GetOrAddComponent<LayoutElement>().enabled = true;
                    gameObject.GetOrAddComponent<LayoutElement>().preferredWidth = maxWidth;
                }
                else
                {
                    gameObject.GetOrAddComponent<LayoutElement>().enabled = false;
                }
            }
            else
            {
                var layoutElement = gameObject.GetComponent<LayoutElement>();
                if (layoutElement != null) layoutElement.enabled = false;
            }
        }

        private void CheckOutlineAndShadow()
        {
            if (_fontSize != fontSize)
            {
                var size = fontSize * _outlinePerFontSize;
                gameObject.GetOrAddComponent<Shadow>().effectDistance = new Vector2(size, -size);
                gameObject.GetOrAddComponent<Outline>().effectDistance = new Vector2(size, -size);
                _fontSize = fontSize;
            }
        }

        static bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c) || c == '\r' || c == '\n';
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            // if (enableLink)
            UpdateText(toFill);
            // if (enableLink)
                SetLinkText(toFill);
        }

        #region 图文混排处理

        /// <summary>
        /// 根据<quad>标签生成图片
        /// </summary>
        void GenerateQuadImage()
        {
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
            {
                return;
            }
#endif

            m_ImageStartVertexIndices.Clear();


            var length = 0;

            var textWithoutWhiteSpace = ProcessText(m_Text);
            foreach (Match match in GetQuadMatches(textWithoutWhiteSpace))
            {
                m_ImageStartVertexIndices.Add((match.Index - length) * 4);
                length += match.Length - 1;
                m_UsedImages.RemoveAll(image => (bool)image);
                if (m_UsedImages.Count == 0)
                {
                    GetComponentsInChildren<Image>(true, m_UsedImages);
                }

                if (m_ImageStartVertexIndices.Count > m_UsedImages.Count)
                {
                    var resources = new DefaultControls.Resources();
                    var go = DefaultControls.CreateImage(resources);
                    go.layer = gameObject.layer;
                    var rt = go.transform as RectTransform;
                    if (rt)
                    {
                        rt.SetParent(rectTransform);
                        rt.localPosition = Vector3.zero;
                        rt.localRotation = Quaternion.identity;
                        rt.localScale = Vector3.one;
                        rt.anchorMax = new Vector2(0, 0.5f);
                        rt.anchorMin = new Vector2(0, 0.5f);
                    }

                    m_UsedImages.Add(go.GetComponent<Image>());
                }

                var spriteName = match.Groups[1].Value;
                var img = m_UsedImages[m_ImageStartVertexIndices.Count - 1];
                if (img.sprite == null || img.sprite.name != spriteName)
                {
                    if (OnLoadSprite != null)
                    {
                        img.sprite = OnLoadSprite(img, spriteName);
                    }
                    else
                    {
                        img.sprite = Resources.Load<Sprite>(spriteName);
                    }
                }


                float size = fontSize;
                Group group = match.Groups[2];
                if (group.Success && match.Value.Contains("size="))
                {
                    size = float.Parse(group.Value);
                }


                img.raycastTarget = false;
                img.rectTransform.sizeDelta = new Vector2(size, size);
                img.preserveAspect = true;
                img.enabled = true;
            }

            for (var i = m_ImageStartVertexIndices.Count; i < m_UsedImages.Count; i++)
            {
                if (m_UsedImages[i])
                {
                    m_UsedImages[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string ProcessText(string text)
        {
            TextBuilder.Length = 0;

            var lastIndex = 0;
            foreach (Match match in GetQuadMatches(text))
            {
                var token = text.Substring(lastIndex, match.Index - lastIndex);
                token = string.Concat(token.Where(c => !IsWhiteSpace(c)));
                TextBuilder.Append(token);
                TextBuilder.Append(match.Value);
                lastIndex = match.Index + match.Length;
            }

            TextBuilder.Append(text.Substring(lastIndex, text.Length - lastIndex));
            return TextBuilder.ToString();
        }

        static MatchCollection GetQuadMatches(string txt)
        {
            if (s_ImageRegex.IsMatch(txt))
            {
                return s_ImageRegex.Matches(txt);
            }

            if (s_ImageRegex1.IsMatch(txt))
            {
                return s_ImageRegex1.Matches(txt);
            }

            if (s_ImageRegex2.IsMatch(txt))
            {
                return s_ImageRegex2.Matches(txt);
            }

            if (s_ImageRegex3.IsMatch(txt))
            {
                return s_ImageRegex3.Matches(txt);
            }

            return s_ImageEmpty.Matches("1");
        }

        /// <summary>
        /// 根据原始字符生成网格,并将Text处理出来的图片UV设置为0,留好空白,将实际加载的图片显示在对应空白处
        /// </summary>
        /// <param name="toFill"></param>
        void UpdateText(VertexHelper toFill)
        {
            if (font == null)
                return;

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            // >>>>> KG Begin
            var orignText = m_Text;

            Vector2 extents = rectTransform.rect.size;
            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(m_Text, settings, gameObject);

            m_Text = orignText;
            // <<<<< KG End

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line... (\n)			

            int vertCount = verts.Count;
            /*
              清除乱码
    
                (1,1)
              0---1
              | \ |
              3---2
            (0,0)
    
             */

            var roundingOffset = Vector2.zero;
            if (vertCount > 0)
            {
                roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
                roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            }

            toFill.Clear();
            int imgIdx = 0;
            var vec2Zero = new Vector4(0, 1, 0, 0);
            var vec2One = new Vector4(1, 1, 0, 0);
            var vec2Two = new Vector4(1, 0, 0, 0);
            var vec2Three = Vector4.zero;
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                    if (tempVertsIndex == 3)
                    {
                        if (m_TempVerts[3].uv0 == vec2Three && m_TempVerts[1].uv0 == vec2One &&
                            m_TempVerts[2].uv0 == vec2Two && m_TempVerts[0].uv0 == vec2Zero)
                        {
                            m_TempVerts[0].uv0 = Vector2.zero;
                            m_TempVerts[1].uv0 = Vector2.zero;
                            m_TempVerts[2].uv0 = Vector2.zero;
                            m_TempVerts[3].uv0 = Vector2.zero;
                            // 更新图片的位置
                            UpdateImgPos(ref imgIdx, m_TempVerts);
                        }

                        toFill.AddUIVertexQuad(m_TempVerts);
                    }
                }
            }
            else
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                    {
                        if (m_TempVerts[3].uv0 == vec2Three && m_TempVerts[1].uv0 == vec2One &&
                            m_TempVerts[2].uv0 == vec2Two && m_TempVerts[0].uv0 == vec2Zero)
                        {
                            m_TempVerts[0].uv0 = Vector2.zero;
                            m_TempVerts[1].uv0 = Vector2.zero;
                            m_TempVerts[2].uv0 = Vector2.zero;
                            m_TempVerts[3].uv0 = Vector2.zero;
                            // 更新图片的位置
                            UpdateImgPos(ref imgIdx, m_TempVerts);
                        }

                        toFill.AddUIVertexQuad(m_TempVerts);
                    }
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
            if (m_ShowImgCount != imgIdx)
            {
                m_ShowImgCount = imgIdx;
                m_ImgDirty = true;
            }
        }

        void UpdateImgPos(ref int imgIdx, UIVertex[] verts)
        {
            var textRect = rectTransform.rect;
            var count = m_UsedImages.Count;
            if (imgIdx >= count)
            {
                return;
            }

            var img = m_UsedImages[imgIdx];
            if (img != null)
            {
                var rt = m_UsedImages[imgIdx].rectTransform;
                var size = rt.sizeDelta;
                var vertPosX = (verts[3].position.x + verts[1].position.x) / 2;
                var vertPosY = (verts[3].position.y + verts[1].position.y) / 2;
                float x = vertPosX; // + size.x / 2;
                if (ShouldFlip)
                {
                    x = x + (textRect.center.x - x) * 2.0f;
                }

                rt.localPosition = new Vector2(x, vertPosY);
                ++imgIdx;
            }
        }

        #endregion


        #region Link

        //------------------------------------------Link部分---------------------------------
        // public static bool enableLink;

        private readonly List<LinkInfo> Links = new List<LinkInfo>();

        public class HrefClickEvent : UnityEvent<string>
        {
        }

        private HrefClickEvent OnHrefClick = new HrefClickEvent();

        public HrefClickEvent onHrefClick
        {
            get { return OnHrefClick; }
            set { OnHrefClick = value; }
        }

        private static readonly Regex s_HrefRegex =
            new Regex(@"<a (href|act)=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

        private void GetOutputText()
        {
            Links.Clear();
            TextBuilder.Length = 0;
            var indexText = 0;
            var textWithoutWhiteSpace = ProcessText_Link(m_Text);
            foreach (Match match in s_HrefRegex.Matches(textWithoutWhiteSpace))
            {
                TextBuilder.Append(textWithoutWhiteSpace.Substring(indexText, match.Index - indexText));
                var token = match.Groups[3].Value;
                token = string.Concat(token.Where(c => !IsWhiteSpace(c)));
                var linkInfo = new LinkInfo
                {
                    startIndex = TextBuilder.Length * 4,
                    endIndex = (TextBuilder.Length + token.Length) * 4,
                    content = match.Groups[2].Value,
                    key = match.Groups[1].Value
                };
                Links.Add(linkInfo);

                TextBuilder.Append(token);
                indexText = match.Index + match.Length;
            }

            // TextBuilder.Append(textWithoutWhiteSpace.Substring(indexText, textWithoutWhiteSpace.Length - indexText));
            // TextBuilder.Length = 0;
            // indexText = 0;
            //
            // foreach (Match match in s_HrefRegex.Matches(outputText))
            // {
            //     TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));
            //     TextBuilder.Append(match.Groups[3].Value);
            //     indexText = match.Index + match.Length;
            // }
            //
            // TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));
            //
            // return TextBuilder.ToString();
        }

        protected static readonly StringBuilder SpaceTextBuilder = new StringBuilder();

        private static string ProcessText_Link(string text)
        {
            text = text.Replace('\r', '\0');

            SpaceTextBuilder.Length = 0;

            var lastIndex = 0;
            foreach (Match match in s_HrefRegex.Matches(text))
            {
                var token = text.Substring(lastIndex, match.Index - lastIndex);
                token = string.Concat(token.Where(c => !IsWhiteSpace(c)));
                SpaceTextBuilder.Append(token);
                SpaceTextBuilder.Append(match.Value);

                lastIndex = match.Index + match.Length;
            }

            SpaceTextBuilder.Append(text.Substring(lastIndex, text.Length - lastIndex));
            return SpaceTextBuilder.ToString();
        }

        private void SetLinkText(VertexHelper toFill)
        {
            var vert = new UIVertex();

            foreach (var hrefInfo in Links)
            {
                hrefInfo.boxes.Clear();
                if (hrefInfo.startIndex >= toFill.currentVertCount)
                {
                    continue;
                }

                toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
                var pos = vert.position;
                var bounds = new Bounds(pos, Vector3.zero);
                for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
                {
                    if (i >= toFill.currentVertCount)
                    {
                        break;
                    }

                    toFill.PopulateUIVertex(ref vert, i);

                    Color defalut = Color.white;
                    ColorUtility.TryParseHtmlString("#60B234FF", out defalut);
                    vert.color = defalut;

                    toFill.SetUIVertex(vert, i);
                    pos = vert.position;

                    if (pos.x < bounds.min.x)
                    {
                        hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos);
                    }
                }

                hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
            }
        }

        private class LinkInfo
        {
            public int startIndex;
            public int endIndex;
            public string content;
            public string key;
            public readonly List<Rect> boxes = new List<Rect>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                    eventData.pressEventCamera, out var lp) && rectTransform.rect.Contains(lp))
            {
                foreach (var hrefInfo in Links)
                {
                    foreach (var box in hrefInfo.boxes)
                    {
                        if (box.Contains(lp))
                        {
                            if (hrefInfo.key.Equals("href"))
                                Application.OpenURL(hrefInfo.content);
                            else
                                OnHrefClick.Invoke(hrefInfo.content);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        private void LateUpdate()
        {
            if (m_ImgDirty)
            {
                var count = m_UsedImages.Count;
                for (int i = 0; i < count; ++i)
                {
                    var img = m_UsedImages[i];
                    if (img != null)
                    {
                        img.enabled = i < m_ShowImgCount;
                    }
                }

                m_ImgDirty = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}