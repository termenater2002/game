using System;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// View Displaying an error message and a retry button
    /// </summary>
    internal class ErrorView : VisualElement
    {
        /// <summary>
        /// Error Visual Element
        /// </summary>
        Text m_ErrorText;

        /// <summary>
        /// Retry Button
        /// </summary>
        ActionButton m_RetryButton;

        /// <summary>
        /// Delete Button
        /// </summary>
        IconButton m_DeleteButton;

        /// <summary>
        /// Current Error string
        /// </summary>
        string m_Error;

        /// <summary>
        /// Error Text Class
        /// </summary>
        const string k_ErrorTextClass = "muse-errorview--text";

        /// <summary>
        /// Retry Button Class
        /// </summary>
        const string k_RetryButtonClass = "muse-errorview--retry-button";

        /// <summary>
        /// Delete Button Class
        /// </summary>
        const string k_DeleteButtonClass = "muse-errorview--delete-button";

        /// <summary>
        /// Delete Button Parent Class
        /// </summary>
        const string k_DeleteButtonParentClass = "muse-errorview--delete-button-parent";


        /// <summary>
        /// On Retry event
        /// </summary>
        public event Action OnRetry;

        /// <summary>
        /// On Delete event
        /// </summary>
        public event Action OnDelete;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ErrorView(StyleSheet styleSheet = null)
        {
            InitializeView(styleSheet);
        }

        /// <summary>
        /// Adding the different views to the ErrorView
        /// </summary>
        void InitializeView(StyleSheet styleSheet = null)
        {
            if (styleSheet == null)
                styleSheet = ResourceManager.Load<StyleSheet>(PackageResources.errorViewStyleSheet);
            styleSheets.Add(styleSheet);

            m_ErrorText = new Text();
            m_ErrorText.AddToClassList(k_ErrorTextClass);
            Add(m_ErrorText);

            m_RetryButton = new ActionButton();
            m_RetryButton.AddToClassList(k_RetryButtonClass);
            m_RetryButton.label = "Retry";
            m_RetryButton.clickable.clicked += OnRetryClicked;
            Add(m_RetryButton);

            var deleteButtonParent = new VisualElement();
            deleteButtonParent.AddToClassList(k_DeleteButtonParentClass);
            deleteButtonParent.pickingMode = PickingMode.Ignore;
            Add(deleteButtonParent);

            m_DeleteButton = new IconButton();
            m_DeleteButton.AddToClassList(k_DeleteButtonClass);
            m_DeleteButton.icon = "delete--regular";
            m_DeleteButton.clickable.clicked += OnDeleteClicked;
            deleteButtonParent.Add(m_DeleteButton);

        }

        /// <summary>
        /// Setting the error message
        /// </summary>
        /// <param name="error">Error message</param>
        public void SetError(string error)
        {
            m_Error = error;
            RefreshErrorText();
        }

        /// <summary>
        /// Refreshing the Text view with the error message
        /// </summary>
        void RefreshErrorText()
        {
            if (m_ErrorText == null)
                return;

            m_ErrorText.text = m_Error;
        }

        void OnDeleteClicked()
        {
            OnDelete?.Invoke();
        }

        void OnRetryClicked()
        {
            OnRetry?.Invoke();
        }
    }
}