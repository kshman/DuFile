namespace DuFile.Windows;

/// <summary>
/// TabStrip 컨트롤은 탭 UI를 제공하는 사용자 지정 컨트롤입니다.
/// 탭 추가/제거, 선택, 스크롤, 닫기 버튼, 탭 목록 메뉴 기능을 지원합니다.
/// </summary>
public class TabStrip : ThemeControl
{
	// 아이콘 크기
	private int _iconSize = 16;
	// 닫기 버튼 크기
	private int _closeButtonSize = 14;
	// 선택된 탭 위에 칠할 강조 사각형의 높이
	private int _activeTabHighlightHeight = 2;
	// 탭 최소/최대 너비
	private int _minTabWidth = 60;
	private int _maxTabWidth = 180;
	// 스크롤 오프셋
	private int _scrollOffset /*= 0*/;
	// 스크롤 버튼 너비
	private int _scrollButtonWidth = 22;
	// 스크롤 버튼 표시 여부
	private bool _showScrollButtons;
	// 활성 상태 여부
	private bool _isActive;

	// 현재 선택된 탭 인덱스
	private int _selectedIndex = -1;
	// 현재 마우스가 올라간 탭 인덱스
	private int _hoverTabIndex = -1;

	// 탭 목록 버튼 영역
	private Rectangle _tabListButtonRect = Rectangle.Empty;
	// 탭 목록 메뉴
	private ContextMenuStrip? _tabListMenu;
	// 탭 목록 버튼 글꼴
	private Font? _tablistFont;
	// 탭 목록 버튼 텍스트 정렬
	private readonly StringFormat _tabListFormat = new()
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	/// <summary>
	/// 선택된 탭 위에 칠할 강조 사각형의 높이(포인트). 디자이너에서 변경 가능.
	/// </summary>
	[Category("TabStrip")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public int ActiveTabHighlightHeight { get => _activeTabHighlightHeight; set { _activeTabHighlightHeight = value; Invalidate(); } }

	/// <summary>
	/// TabStrip에 표시되는 탭 목록을 가져옵니다.
	/// </summary>
	//[Category("TabStrip")]
	//[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<TabItem> Tabs { get; } = [];

	/// <summary>
	/// 현재 탭 개수를 가져옵니다.
	/// </summary>
	[Browsable(false)]
	public int Count => Tabs.Count;

	/// <summary>
	/// 현재 선택된 탭을 가져옵니다.
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
	/// 현재 컨트롤이 활성 상태인지 가져오거나 설정합니다.
	/// </summary>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsActive
	{
		get => _isActive;
		set
		{
			_isActive = value;
			Invalidate();
		}
	}

	/// <summary>
	/// 선택된 탭이 변경될 때 발생합니다.
	/// </summary>
	[Category("TabStrip")]
	public event EventHandler<TabStripIndexChangedEventArgs>? SelectedIndexChanged;

	/// <summary>
	/// 선택된 탭의 닫기 버튼이 클릭될 때 발생합니다.
	/// </summary>
	[Category("TabStrip")]
	public event EventHandler<TabStripCloseClickEventArgs>? CloseClick;

	/// <summary>
	/// 탭, 닫기, 스크롤, 목록 등 클릭 시 발생하는 이벤트입니다.
	/// </summary>
	[Category("TabStrip")]
	public event EventHandler<TabStripElementClickEventArgs>? ElementClick;

	/// <summary>
	/// TabStrip의 인스턴스를 초기화합니다.
	/// </summary>
	public TabStrip()
	{
		SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
		Height = 28;
	}

	/// <inheritdoc />
	protected override void OnUpdateTheme(Theme theme)
	{
		_tablistFont = new Font(theme.UiFontFamily, 13.0f, FontStyle.Bold, GraphicsUnit.Point);
	}

	/// <inheritdoc />
	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (IsReallyDesignMode)
		{
			// 디자인 모드에서 샘플 탭 추가
			AddTab("디자인모드");
			AddTab("추가 탭");
			AddTab("하나 더");
		}
	}

	/// <summary>
	/// 지정한 텍스트, 값, 아이콘으로 새 탭을 추가합니다.
	/// </summary>
	/// <param name="text">탭에 표시할 텍스트</param>
	/// <param name="value">탭에 연결할 값(선택)</param>
	/// <param name="icon">탭에 표시할 아이콘(선택)</param>
	/// <returns>추가된 탭의 인덱스</returns>
	public int AddTab(string text, object? value = null, Image? icon = null)
	{
		Tabs.Add(new TabItem { Text = text, Icon = icon, Value = value });
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
	/// Removes all tabs except the tab at the specified index.
	/// </summary>
	/// <remarks>If the specified index is out of range or if there is only one tab, no action is taken.  After
	/// removal, the specified tab becomes the selected tab.</remarks>
	/// <param name="index">The zero-based index of the tab to retain. Must be within the range of existing tabs.</param>
	public void RemoveOtherTabs(int index)
	{
		if (Tabs.Count <= 1 || index < 0 || index >= Tabs.Count)
			return; // 최소 1개 탭은 유지

		var selectedTab = Tabs[index];
		Tabs.Clear();
		Tabs.Add(selectedTab);
		SelectedIndex = 0;  // Invalidate()는 이 안에서 호출
	}

	/// <summary>
	/// 지정한 인덱스의 탭을 반환합니다.
	/// </summary>
	/// <param name="index">탭 인덱스</param>
	/// <returns>탭 객체 또는 null</returns>
	public TabItem? GetTabAt(int index)
	{
		if (index < 0 || index >= Tabs.Count)
			return null;
		return Tabs[index];
	}

	/// <summary>
	/// 지정한 값과 일치하는 첫 번째 탭의 인덱스를 반환합니다.
	/// </summary>
	/// <param name="value">탭에 연결된 값</param>
	/// <returns>탭 인덱스 또는 -1</returns>
	public int GetTabIndexByValue(object? value)
	{
		for (var i = 0; i < Tabs.Count; i++)
		{
			if (Equals(Tabs[i].Value, value))
				return i;
		}
		return -1; // 찾지 못함
	}

	/// <summary>
	/// 지정한 인덱스의 탭 텍스트와 태그를 설정합니다.
	/// </summary>
	/// <param name="index">탭 인덱스</param>
	/// <param name="text">새 텍스트</param>
	/// <param name="tag">연결할 태그(선택)</param>
	public void SetTabTextAt(int index, string text, object? tag = null)
	{
		if (index < 0 || index >= Tabs.Count)
			return;

		var tab = Tabs[index];
		tab.Text = text;
		tab.Value = tag;
		Invalidate();
	}

	/// <summary>
	/// 모든 탭의 텍스트 목록을 반환합니다.
	/// </summary>
	/// <returns>텍스트 목록</returns>
	public List<string> GetTextList() =>
		(from tab in Tabs where !string.IsNullOrEmpty(tab.Text) select tab.Text).ToList();

	/// <summary>
	/// 모든 탭의 값 목록을 반환합니다.
	/// </summary>
	/// <returns>값 목록</returns>
	public List<object> GetValueList() =>
		(from tab in Tabs where tab.Value != null select tab.Value).ToList();

	/// <inheritdoc />
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		var theme = Settings.Instance.Theme;
		e.Graphics.Clear(theme.Background);

		const int tabListBtnW = 22;
		_tabListButtonRect = new Rectangle(2, 2, tabListBtnW, Height - 4);

		using (var b = new SolidBrush(theme.BackContent))
			e.Graphics.FillRectangle(b, _tabListButtonRect);
		ControlPaint.DrawBorder(e.Graphics, _tabListButtonRect, theme.Border, ButtonBorderStyle.Solid);

		// "≡" 문자로 목록 버튼 아이콘
		using (var brush = new SolidBrush(theme.Foreground))
		{
			var font = _tablistFont ?? Font;
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			e.Graphics.DrawString("≡", font, brush, _tabListButtonRect, _tabListFormat);
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
			var tabWidth = Math.Max(_minTabWidth, Math.Min(textSize.Width + 20 + iconSpace + _closeButtonSize + 6, _maxTabWidth));

			var tabRect = new Rectangle(x, 2, tabWidth, Height - 4);
			tab.Bounds = tabRect;

			var tabBack =
				i == _selectedIndex ? _isActive ? theme.BackWindowActive : theme.BackSelection :
				i == _hoverTabIndex ? theme.BackContent : theme.Background;
			using (var tabBrush = new SolidBrush(tabBack))
				e.Graphics.FillRectangle(tabBrush, tabRect);

			ControlPaint.DrawBorder(e.Graphics, tabRect, theme.Border, ButtonBorderStyle.Solid);

			if (i == _selectedIndex && ActiveTabHighlightHeight > 0)
			{
				using var highlightBrush = new SolidBrush(theme.BackActive);
				e.Graphics.FillRectangle(highlightBrush, tabRect.Left, tabRect.Top, tabRect.Width, ActiveTabHighlightHeight);
			}

			var drawX = tabRect.Left + 8;
			if (tab.Icon != null)
			{
				e.Graphics.DrawImage(tab.Icon, drawX, tabRect.Top + (tabRect.Height - _iconSize) / 2, _iconSize, _iconSize);
				drawX += _iconSize + 4;
			}

			var textRect = new Rectangle(drawX, tabRect.Top, tabRect.Width - (drawX - tabRect.Left) - 2 - (_closeButtonSize + 6), tabRect.Height);

			// 텍스트 ... 처리
			TextRenderer.DrawText(e.Graphics, tab.Text, Font, textRect, theme.Foreground,
				TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

			// 모든 탭에 닫기 버튼 위치 계산
			var closeRect = new Rectangle(
				tabRect.Right - _closeButtonSize - 4,
				tabRect.Top + (tabRect.Height - _closeButtonSize) / 2,
				_closeButtonSize,
				_closeButtonSize
			);
			tab.CloseButtonBounds = closeRect;

			if (_isActive && i == _selectedIndex && Tabs.Count > 1)
			{
				using (var closeBrush = new SolidBrush(theme.Accelerator))
					e.Graphics.FillEllipse(closeBrush, closeRect);
				using (var pen = new Pen(theme.Foreground, 2))
				{
					const int pad = 3;
					e.Graphics.DrawLine(pen, closeRect.Left + pad, closeRect.Top + pad, closeRect.Right - pad, closeRect.Bottom - pad);
					e.Graphics.DrawLine(pen, closeRect.Right - pad, closeRect.Top + pad, closeRect.Left + pad, closeRect.Bottom - pad);
				}
				using (var pen = new Pen(theme.Border))
					e.Graphics.DrawEllipse(pen, closeRect);
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

	// 마우스 위치에 따라 hover 탭 인덱스를 계산하고, 필요시 Invalidate합니다.
	private void UpdateHoverTabIndex(Point mouseLocation)
	{
		var newHover = -1;
		for (var i = 0; i < Tabs.Count; i++)
		{
			if (Tabs[i].Bounds.Contains(mouseLocation))
			{
				newHover = i;
				break;
			}
		}
		if (_hoverTabIndex != newHover)
		{
			_hoverTabIndex = newHover;
			Invalidate();
		}
	}

	/// <inheritdoc />
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		UpdateHoverTabIndex(e.Location);
	}

	/// <inheritdoc />
	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hoverTabIndex != -1)
		{
			_hoverTabIndex = -1;
			Invalidate();
		}
	}

	/// <inheritdoc />
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();

		UpdateHoverTabIndex(e.Location);

		// 탭 목록 버튼 클릭 체크
		if (_tabListButtonRect.Contains(e.Location))
		{
			if (!InvokeClicked(e, TabStripElement.List) && e.Button == MouseButtons.Left)
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
				if (!InvokeClicked(e, TabStripElement.LeftScroll))
				{
					_scrollOffset = Math.Max(0, _scrollOffset - scrollStep);
					Invalidate();
				}
				return;
			}

			if (rightBtn.Contains(e.Location))
			{
				if (!InvokeClicked(e, TabStripElement.RightScroll))
				{
					var totalTabsWidth = GetTotalTabsWidth();
					var maxOffset = Math.Max(0, totalTabsWidth - (Width - _scrollButtonWidth * 2 - tabListBtnW - 4));
					_scrollOffset = Math.Min(maxOffset, _scrollOffset + scrollStep);
					Invalidate();
				}
				return;
			}
		}

		// 탭 클릭
		for (var i = 0; i < Tabs.Count; i++)
		{
			var tab = Tabs[i];

			// 닫기 버튼
			if (e.Button == MouseButtons.Left && Tabs.Count > 1 && tab.CloseButtonBounds.Contains(e.Location))
			{
				if (!InvokeClicked(e, TabStripElement.Close, i))
					CloseClick?.Invoke(this, new TabStripCloseClickEventArgs(i));
				return;
			}

			// 탭
			if (tab.Bounds.Contains(e.Location))
			{
				if (!InvokeClicked(e, TabStripElement.Tab, i))
					SelectedIndex = i;
				return;
			}
		}

		// 빈 곳 클릭
		InvokeClicked(e, TabStripElement.None);
	}

	/// <summary>
	/// 탭 목록 메뉴 표시
	/// </summary>
	public void ShowTabListMenu()
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

	// 모든 탭의 총 너비를 계산합니다.
	private int GetTotalTabsWidth()
	{
		var x = 0;
		foreach (var tab in Tabs)
		{
			var iconSpace = (tab.Icon != null) ? (_iconSize + 4) : 0;
			var textSize = TextRenderer.MeasureText(tab.Text, Font);
			var tabWidth = Math.Max(_minTabWidth, Math.Min(textSize.Width + 20 + iconSpace + _closeButtonSize + 6, _maxTabWidth));
			x += tabWidth + 2;
		}
		return x;
	}

	// 스크롤 버튼을 그립니다.
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

	// 클릭 이벤트를 발생시키고, 핸들링 여부를 반환합니다.
	private bool InvokeClicked(MouseEventArgs mArgs, TabStripElement element, int index = -1)
	{
		if (ElementClick == null)
			return false;
		var args = new TabStripElementClickEventArgs(mArgs.Location, mArgs.Button, element, index);
		ElementClick.Invoke(this, args);
		return args.Handled;
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
		/// 탭에 연결된 데이터를 가져오거나 설정합니다.
		/// </summary>
		public object? Value { get; set; }

		/// <inheritdoc />
		public override string ToString() => Text;
	}
}

/// <summary>
/// 탭 닫기 버튼 클릭 이벤트 인수입니다.
/// </summary>
public class TabStripCloseClickEventArgs(int index) : EventArgs
{
	/// <summary>
	/// 닫기 버튼이 클릭된 탭의 인덱스입니다.
	/// </summary>
	public int Index { get; } = index;
}

/// <summary>
/// 탭 인덱스 변경 이벤트 인수입니다.
/// </summary>
/// <param name="prevIndex">이전 인덱스</param>
/// <param name="index">새 인덱스</param>
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

/// <summary>
/// TabStrip의 클릭 요소 종류입니다.
/// </summary>
public enum TabStripElement
{
	/// <summary>아무것도 아님</summary>
	None,
	/// <summary>목록 버튼</summary>
	List,
	/// <summary>왼쪽 스크롤 버튼</summary>
	LeftScroll,
	/// <summary>오른쪽 스크롤 버튼</summary>
	RightScroll,
	/// <summary>닫기 버튼</summary>
	Close,
	/// <summary>탭</summary>
	Tab,
}

/// <summary>
/// TabStrip 클릭 이벤트 인수입니다.
/// </summary>
public class TabStripElementClickEventArgs(Point location, MouseButtons button, TabStripElement element, int index = -1) : EventArgs
{
	/// <summary>
	/// 클릭 위치
	/// </summary>
	public Point Location { get; } = location;

	/// <summary>
	/// 마우스 버튼
	/// </summary>
	public MouseButtons Button { get; } = button;

	/// <summary>
	/// 클릭된 요소
	/// </summary>
	public TabStripElement Element { get; set; } = element;

	/// <summary>
	/// 클릭된 탭 인덱스
	/// </summary>
	public int Index { get; } = index;

	/// <summary>
	/// 이벤트가 처리되었는지 여부 (true면 기본 동작 안함)
	/// </summary>
	public bool Handled { get; set; } = false;
}
