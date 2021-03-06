﻿using System;
using AGS.API;

namespace AGS.Engine
{
    public class StringPropertyEditor : IInspectorPropertyEditor
    {
        private readonly IGameFactory _factory;
        private InspectorProperty _property;
        private readonly bool _enabled;
        private ITextComponent _textbox;

        public StringPropertyEditor(IGameFactory factory, bool enabled)
        {
            _factory = factory;
            _enabled = enabled;
        }

        public void AddEditorUI(string id, ITreeNodeView view, InspectorProperty property)
        {
            _property = property;
			var label = view.TreeItem;
            var config = _enabled ? GameViewColors.TextConfig : GameViewColors.ReadonlyTextConfig;
			var textbox = _factory.UI.GetTextBox(id,
												 label.X, label.Y, label.TreeNode.Parent,
												 "", config, width: 100f, height: 20f);
            textbox.Text = property.Value;
            textbox.TextBackgroundVisible = false;
            textbox.Enabled = _enabled;
            if (_enabled)
            {
                GameViewColors.AddHoverEffect(textbox);
            }
            _textbox = textbox;
			textbox.RenderLayer = label.RenderLayer;
			textbox.Z = label.Z;
            HoverEffect.Add(textbox, Colors.Transparent, Colors.DarkSlateBlue);
			textbox.OnPressingKey.Subscribe(args =>
			{
                if (args.PressedKey != Key.Enter) return;
                args.ShouldCancel = true;
                textbox.IsFocused = false;
                property.Prop.SetValue(property.Object, textbox.Text);
			});
            textbox.LostFocus.Subscribe(args => {
                property.Prop.SetValue(property.Object, textbox.Text);
            });
        }

        public void RefreshUI()
        {
            if (_textbox == null) return;
            _textbox.Text = _property.Value;
        }
    }
}
