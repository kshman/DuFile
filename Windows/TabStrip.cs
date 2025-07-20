namespace DuFile.Windows;

/// <summary>
/// TabStrip 컨트롤은 탭 UI를 제공하는 사용자 지정 컨트롤입니다.
/// 탭 추가/제거, 선택, 스크롤, 닫기 버튼, 탭 목록 메뉴 기능을 지원합니다.
/// </summary>
public class TabStrip : Control
{
	private int _selectedIndex = -1;

	private int _closeButtonSize = 14;
	private int _iconSize = 16;

	private int _minTabWidth = 60;
	private int _maxTabWidth = 180;
	private int _scrollOffset /*= 0*/;
	private int _scrollButtonWidth = 22;
	private bool _showScrollButtons;

	private Rectangle _tabListButtonRect = Rectangle.Empty;
	private ContextMenuStrip? _tabListMenu;

	/// <summary>
	/// TabStrip에 표시되는 탭 목록을 가져옵니다.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public List<TabItem> Tabs { get; } = [];

	/// <summary>
	/// Gets the number of tabs currently available.
	/// </summary>
	[Browsable(false)]
	public int Count => Tabs.Count;

	/// <summary>
	/// Gets the currently selected tab item.
	/// </summary>
	[Browsable(false)]
	public TabItem? SelectedTab => _selectedIndex >= 0 && _selectedIndex < Tabs.Count ? Tabs[_selectedIndex] : null;

	/// <summary>
	/// 닫기 버튼의 크기를 가져오거나 설정합니다.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int CloseButtonSize { get => _closeButtonSize; set { _closeButtonSize = value; Invalidate(); } }

	/// <summary>
	/// 탭 아이콘의 크기를 가져오거나 설정합니다.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int IconSize { get => _iconSize; set { _iconSize = value; Invalidate(); } }

	/// <summary>
	/// 탭의 최소 너비를 가져오거나 설정합니다.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int MinTabWidth { get => _minTabWidth; set { _minTabWidth = value; Invalidate(); } }

	/// <summary>
	/// 탭의 최대 너비를 가져오거나 설정합니다.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int MaxTabWidth { get => _maxTabWidth; set { _maxTabWidth = value; Invalidate(); } }

	/// <summary>
	/// 스크롤 버튼의 너비를 가져오거나 설정합니다.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int ScrollButtonWidth { get => _scrollButtonWidth; set { _scrollButtonWidth = value; Invalidate(); } }

	/// <summary>
	/// 현재 선택된 탭의 인덱스를 가져오거나 설정합니다. -1이면 선택 없음.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectedIndex
	{
		get => _selectedIndex;
		set
		{
			var prevIndex = _selectedIndex;
			if (value >= -1 && value < Tabs.Count)
			{
				_selectedIndex = value;
				Invalidate();
				SelectedIndexChanged?.Invoke(this, new TabStripIndexChangedEventArgs(prevIndex, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets the text content stored by the component.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Value { get; set; } = string.Empty;

	/// <summary>
	/// 선택된 탭이 변경될 때 발생합니다.
	/// </summary>
	[Category("TabStrip")]
	public event EventHandler<TabStripIndexChangedEventArgs>? SelectedIndexChanged;

	/// <summary>
	/// 선택된 탭의 닫기 버튼이 클릭될 때 발생합니다.
	/// </summary>
	[Category("TabStrip")]
	public event EventHandler<TabStripCloseEventArgs>? CloseButtonClicked;

	/// <summary>
	/// TabStrip의 인스턴스를 초기화합니다.
	/// </summary>
	public TabStrip()
	{
		SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		Height = 28;
	}

	/// <summary>
	/// 새 탭을 추가합니다.
	/// </summary>
	/// <param name="text">탭에 표시할 텍스트</param>
	/// <param name="icon">탭에 표시할 아이콘(선택)</param>
	public int AddTab(string text, Image? icon = null)
	{
		Tabs.Add(new TabItem { Text = text, Icon = icon });
		if (_selectedIndex == -1)
			SelectedIndex = 0;
		Invalidate();
		return Tabs.Count - 1; // 새로 추가된 탭의 인덱스 반환
	}

	/// <summary>
	/// Adds a new tab with the specified text, tag, and optional icon to the collection of tabs.
	/// </summary>
	/// <remarks>If this is the first tab being added, it automatically becomes the selected tab.</remarks>
	/// <param name="text">The text to display on the tab. Cannot be null or empty.</param>
	/// <param name="tag">An optional object associated with the tab for identification or data storage. Can be null.</param>
	/// <param name="icon">An optional image to display on the tab. Can be null.</param>
	public int AddTabWithTag(string text, object? tag, Image? icon = null)
	{
		Tabs.Add(new TabItem { Text = text, Icon = icon, Tag = tag });
		if (_selectedIndex == -1)
			SelectedIndex = 0;
		Invalidate();
		return Tabs.Count - 1; // 새로 추가된 탭의 인덱스 반환
	}

	/// <summary>
	/// 지정한 인덱스의 탭을 제거합니다.
	/// </summary>
	/// <param name="index">제거할 탭의 인덱스</param>
	public void RemoveTabAt(int index)
	{
		if (Tabs.Count == 1)
			return; // 최소 1개 탭은 유지

		if (index >= 0 && index < Tabs.Count)
		{
			Tabs.RemoveAt(index);
			if (_selectedIndex >= Tabs.Count)
				SelectedIndex = Tabs.Count - 1;
			Invalidate();
		}
	}

	/// <summary>
	/// Retrieves the <see cref="TabItem"/> at the specified index in the collection.
	/// </summary>
	/// <param name="index">The zero-based index of the <see cref="TabItem"/> to retrieve.</param>
	/// <returns>The <see cref="TabItem"/> at the specified index, or <see langword="null"/> if the index is out of range.</returns>
	public TabItem? GetTabItemAt(int index)
	{
		if (index < 0 || index >= Tabs.Count)
			return null;
		return Tabs[index];
	}

	/// <summary>
	/// Retrieves the first <see cref="TabItem"/> from the collection that matches the specified tag.
	/// </summary>
	/// <param name="tag">The tag to search for. Can be <see langword="null"/>.</param>
	/// <returns>The first <see cref="TabItem"/> with a matching tag, or <see langword="null"/> if no match is found.</returns>
	public int GetTabIndexByTag(object? tag)
	{
		for (var i = 0; i < Tabs.Count; i++)
		{
			if (Equals(Tabs[i].Tag, tag))
				return i;
		}
		return -1; // 찾지 못함
	}

	/// <summary>
	/// Sets the text and optional tag for a tab at the specified index.
	/// </summary>
	/// <remarks>If the specified <paramref name="index"/> is out of range, the method does nothing.</remarks>
	/// <param name="index">The zero-based index of the tab to update. Must be within the range of existing tabs.</param>
	/// <param name="text">The new text to display on the tab.</param>
	/// <param name="tag">An optional object to associate with the tab. Can be <see langword="null"/>.</param>
	public void SetTabText(int index, string text, object? tag = null)
	{
		if (index < 0 || index >= Tabs.Count)
			return;

		var tab = Tabs[index];
		tab.Text = text;
		tab.Tag = tag;
		Invalidate();
	}

	/// <summary>
	/// Retrieves a list of non-empty text values from the collection of tabs.
	/// </summary>
	/// <returns>A list of strings containing the text of each tab that has a non-empty text value.</returns>
	public List<string> GetTextList() =>
		(from tab in Tabs where !string.IsNullOrEmpty(tab.Text) select tab.Text).ToList();

	/// <summary>
	/// Retrieves a list of non-null tags from the collection of tabs.
	/// </summary>
	/// <returns>A list of objects representing the tags of tabs that have non-null tags. The list will be empty if no tabs have
	/// tags.</returns>
	public List<object> GetTagList() =>
		(from tab in Tabs where tab.Tag != null select tab.Tag).ToList();

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.Clear(BackColor);

		var settings = Settings.Instance;

		const int tabListBtnW = 22;
		_tabListButtonRect = new Rectangle(2, 2, tabListBtnW, Height - 4);

		using (Brush b = new SolidBrush(Color.LightGray))
			e.Graphics.FillRectangle(b, _tabListButtonRect);
		ControlPaint.DrawBorder(e.Graphics, _tabListButtonRect, Color.Gray, ButtonBorderStyle.Solid);

		// "≡" 문자로 목록 버튼 아이콘
		using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using (var font = new Font(settings.UiFontFamily, 13, FontStyle.Bold))
		using (Brush brush = new SolidBrush(Color.Black))
		{
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			e.Graphics.DrawString("≡", font, brush, _tabListButtonRect, sf);
		}

		// 스크롤 버튼 표시 여부 결정
		var totalTabsWidth = GetTotalTabsWidth();
		_showScrollButtons = totalTabsWidth > (Width - tabListBtnW - 4);

		var leftX = tabListBtnW + 2;
		if (_showScrollButtons)
			leftX += _scrollButtonWidth;

		var x = leftX - _scrollOffset;

		for (var i = 0; i < Tabs.Count; i++)
		{
			var tab = Tabs[i];
			var iconSpace = (tab.Icon != null) ? (_iconSize + 4) : 0;

			var textSize = TextRenderer.MeasureText(tab.Text, Font);
			var tabWidth = Math.Max(_minTabWidth, Math.Min(textSize.Width + 20 + iconSpace, _maxTabWidth));

			if (i == _selectedIndex)
				tabWidth += _closeButtonSize + 6;

			var tabRect = new Rectangle(x, 2, tabWidth, Height - 4);
			tab.Bounds = tabRect;

			using (Brush b = new SolidBrush(i == _selectedIndex ? Color.LightSteelBlue : Color.Gainsboro))
				e.Graphics.FillRectangle(b, tabRect);

			ControlPaint.DrawBorder(e.Graphics, tabRect, Color.Gray, ButtonBorderStyle.Solid);

			var drawX = tabRect.Left + 8;
			if (tab.Icon != null)
			{
				e.Graphics.DrawImage(tab.Icon, drawX, tabRect.Top + (tabRect.Height - _iconSize) / 2, _iconSize, _iconSize);
				drawX += _iconSize + 4;
			}

			var textRect = new Rectangle(drawX, tabRect.Top, tabRect.Width - (drawX - tabRect.Left) - 2, tabRect.Height);
			if (i == _selectedIndex)
				textRect.Width -= (_closeButtonSize + 6);

			// 텍스트 ... 처리
			TextRenderer.DrawText(e.Graphics, tab.Text, Font, textRect, ForeColor,
				TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

			// 닫기 버튼
			if (i == _selectedIndex && Tabs.Count > 1)
			{
				var closeRect = new Rectangle(
					tabRect.Right - _closeButtonSize - 4,
					tabRect.Top + (tabRect.Height - _closeButtonSize) / 2,
					_closeButtonSize,
					_closeButtonSize
				);
				tab.CloseButtonBounds = closeRect;
				using (Brush closeBrush = new SolidBrush(Color.White))
					e.Graphics.FillEllipse(closeBrush, closeRect);
				using (var pen = new Pen(Color.DimGray, 2))
				{
					const int pad = 3;
					e.Graphics.DrawLine(pen, closeRect.Left + pad, closeRect.Top + pad, closeRect.Right - pad, closeRect.Bottom - pad);
					e.Graphics.DrawLine(pen, closeRect.Right - pad, closeRect.Top + pad, closeRect.Left + pad, closeRect.Bottom - pad);
				}
				e.Graphics.DrawEllipse(Pens.Gray, closeRect);
			}
			else
			{
				tab.CloseButtonBounds = Rectangle.Empty;
			}

			x += tabWidth + 2;
		}

		// 스크롤 버튼 그리기
		if (_showScrollButtons)
		{
			// ◀
			var leftBtn = new Rectangle(tabListBtnW + 2, 2, _scrollButtonWidth, Height - 4);
			DrawScrollButton(e.Graphics, leftBtn, false, _scrollOffset > 0);

			// ▶
			var rightBtn = new Rectangle(Width - _scrollButtonWidth - 2, 2, _scrollButtonWidth, Height - 4);
			var canScrollRight = (totalTabsWidth - _scrollOffset) > (Width - _scrollButtonWidth * 2 - tabListBtnW - 4);
			DrawScrollButton(e.Graphics, rightBtn, true, canScrollRight);
		}
	}

	private static void DrawScrollButton(Graphics g, Rectangle rect, bool right, bool enabled)
	{
		var bg = enabled ? Color.LightGray : Color.Gainsboro;
		using (Brush b = new SolidBrush(bg))
			g.FillRectangle(b, rect);
		ControlPaint.DrawBorder(g, rect, Color.Gray, ButtonBorderStyle.Solid);

		// 삼각형
		var points = right ?
			new[] { new Point(rect.Left + 6, rect.Top + rect.Height / 2 - 5), new Point(rect.Right - 6, rect.Top + rect.Height / 2), new Point(rect.Left + 6, rect.Top + rect.Height / 2 + 5) } :
			new[] { new Point(rect.Right - 6, rect.Top + rect.Height / 2 - 5), new Point(rect.Left + 6, rect.Top + rect.Height / 2), new Point(rect.Right - 6, rect.Top + rect.Height / 2 + 5) };

		using (Brush f = new SolidBrush(enabled ? Color.Black : Color.Gray))
			g.FillPolygon(f, points);
	}

	private int GetTotalTabsWidth()
	{
		var x = 0;
		for (var i = 0; i < Tabs.Count; i++)
		{
			var tab = Tabs[i];
			var iconSpace = (tab.Icon != null) ? (_iconSize + 4) : 0;
			var textSize = TextRenderer.MeasureText(tab.Text, Font);
			var tabWidth = Math.Max(_minTabWidth, Math.Min(textSize.Width + 20 + iconSpace, _maxTabWidth));
			if (i == _selectedIndex)
				tabWidth += _closeButtonSize + 6;
			x += tabWidth + 2;
		}
		return x;
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		// 탭 목록 버튼 클릭 체크
		if (_tabListButtonRect.Contains(e.Location))
		{
			ShowTabListMenu();
			return;
		}

		// 스크롤 버튼 클릭
		const int tabListBtnW = 22;
		if (_showScrollButtons)
		{
			var leftBtn = new Rectangle(tabListBtnW + 2, 2, _scrollButtonWidth, Height - 4);
			var rightBtn = new Rectangle(Width - _scrollButtonWidth - 2, 2, _scrollButtonWidth, Height - 4);
			const int scrollStep = 60;

			if (leftBtn.Contains(e.Location) && _scrollOffset > 0)
			{
				_scrollOffset = Math.Max(0, _scrollOffset - scrollStep);
				Invalidate();
				return;
			}

			if (rightBtn.Contains(e.Location))
			{
				var totalTabsWidth = GetTotalTabsWidth();
				var maxOffset = Math.Max(0, totalTabsWidth - (Width - _scrollButtonWidth * 2 - tabListBtnW - 4));
				_scrollOffset = Math.Min(maxOffset, _scrollOffset + scrollStep);
				Invalidate();
				return;
			}
		}

		// 탭 클릭
		for (var i = 0; i < Tabs.Count; i++)
		{
			var tab = Tabs[i];
			// 닫기 버튼
			if (i == _selectedIndex && tab.CloseButtonBounds.Contains(e.Location) && Tabs.Count > 1)
			{
				CloseButtonClicked?.Invoke(this, new TabStripCloseEventArgs(i));
				return;
			}
			// 탭
			if (tab.Bounds.Contains(e.Location))
			{
				SelectedIndex = i;
				break;
			}
		}
	}

	private void ShowTabListMenu()
	{
		if (_tabListMenu != null)
		{
			_tabListMenu.Dispose();
			_tabListMenu = null;
		}

		_tabListMenu = new ContextMenuStrip();
		for (var i = 0; i < Tabs.Count; i++)
		{
			var idx = i; // for closure
			var item = new ToolStripMenuItem(Tabs[i].Text)
			{
				Checked = (i == _selectedIndex),
				Image = Tabs[i].Icon // 있으면 아이콘도 표시
			};
			item.Click += (_, _) => { SelectedIndex = idx; };
			_tabListMenu.Items.Add(item);
		}
		_tabListMenu.Show(this, new Point(_tabListButtonRect.Left, _tabListButtonRect.Bottom));
	}

	/// <inheritdoc />
	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		Invalidate();
	}

	/// <inheritdoc />
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Invalidate();
	}

	/// <summary>
	/// TabStrip에 표시되는 단일 탭 정보를 나타냅니다.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class TabItem
	{
		/// <summary>
		/// 탭에 표시할 텍스트를 가져오거나 설정합니다.
		/// </summary>
		public string Text { get; set; } = string.Empty;

		/// <summary>
		/// 탭에 표시할 아이콘을 가져오거나 설정합니다.
		/// </summary>
		public Image? Icon { get; set; }

		/// <summary>
		/// 탭의 화면상 위치와 크기를 가져오거나 설정합니다.
		/// </summary>
		[Browsable(false)]
		public Rectangle Bounds { get; set; }

		/// <summary>
		/// 닫기 버튼의 위치와 크기를 가져오거나 설정합니다.
		/// </summary>
		[Browsable(false)]
		public Rectangle CloseButtonBounds { get; set; }

		/// <summary>
		/// Gets or sets an object that contains data about the control.
		/// </summary>
		public object? Tag { get; set; }

		/// <inheritdoc />
		public override string ToString() => Text;
	}
}

/// <summary>
/// 탭 닫기 버튼 클릭 이벤트 인수입니다.
/// </summary>
public class TabStripCloseEventArgs(int index) : EventArgs
{
	/// <summary>
	/// 닫기 버튼이 클릭된 탭의 인덱스입니다.
	/// </summary>
	public int Index { get; } = index;
}

/// <summary>
/// Provides data for the event that occurs when the index of a tab strip changes.
/// </summary>
/// <remarks>This class contains information about the previous and new index of the tab strip, allowing event
/// handlers to determine how the tab selection has changed.</remarks>
/// <param name="prevIndex"></param>
/// <param name="index"></param>
public class TabStripIndexChangedEventArgs(int prevIndex, int index) : EventArgs
{
	/// <summary>
	/// 이전 탭 인덱스입니다.
	/// </summary>
	public int PrevIndex { get; } = prevIndex;

	/// <summary>
	/// 새 탭 인덱스입니다.
	/// </summary>
	public int Index { get; } = index;
}
