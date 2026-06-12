object FrmComArmadas: TFrmComArmadas
  Left = 307
  Top = 0
  HelpContext = 1
  BorderStyle = bsDialog
  Caption = 'Comisiones armadas'
  ClientHeight = 436
  ClientWidth = 667
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  KeyPreview = True
  OldCreateOrder = False
  Position = poScreenCenter
  OnActivate = FormActivate
  OnClose = FormClose
  OnCreate = FormCreate
  OnKeyPress = FormKeyPress
  OnPaint = FormPaint
  PixelsPerInch = 96
  TextHeight = 13
  object Image1: TImage
    Left = 344
    Top = 184
    Width = 16
    Height = 16
    Picture.Data = {
      07544269746D6170F6000000424DF60000000000000076000000280000001000
      0000100000000100040000000000800000000000000000000000100000000000
      0000000000000000800000800000008080008000000080008000808000008080
      8000C0C0C0000000FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFF
      FF00555555555555555555555555555555555555590555555555555599905555
      5555555599905555555555599999055555555599999905555555579905999055
      5555790555599055555555555559990555555555555599055555555555555990
      5555555555555579055555555555555790555555555555555990555555555555
      5555}
    Stretch = True
    Transparent = True
    Visible = False
  end
  object GroupBox1: TGroupBox
    Left = 0
    Top = 0
    Width = 667
    Height = 177
    HelpContext = 2
    Align = alTop
    Caption = 'Comisiones'
    TabOrder = 0
    object GrillaComi: TDBGrid
      Left = 2
      Top = 15
      Width = 663
      Height = 160
      HelpContext = 3
      Align = alClient
      DataSource = DataSource1
      Options = [dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgRowSelect, dgCancelOnExit]
      TabOrder = 0
      TitleFont.Charset = DEFAULT_CHARSET
      TitleFont.Color = clWindowText
      TitleFont.Height = -11
      TitleFont.Name = 'MS Sans Serif'
      TitleFont.Style = []
      OnDblClick = ModificarClick
      Columns = <
        item
          Expanded = False
          FieldName = 'CUTUCO'
          Title.Caption = 'Comisi'#243'n'
          Width = 50
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'COD_MAT'
          Title.Caption = 'C'#243'digo'
          Width = 45
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'SIGLA'
          Title.Caption = 'Materia'
          Width = 110
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'CUA_ANIO'
          Title.Caption = 'Cuat./A'#241'o'
          Width = 53
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'CODPROFES'
          Title.Caption = 'Cod. Profosor'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'DOCENTE'
          Title.Caption = 'Docente'
          Width = 110
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'TIT_SUP'
          Title.Caption = 'Titular/Suplente'
          Width = 80
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'DIA1'
          Title.Caption = 'Primer d'#237'a'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'BLOQUE1'
          Title.Caption = 'Bloque'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'DIA2'
          Title.Caption = 'Segundo Dia'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'BLOQUE2'
          Title.Caption = 'Bloque'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'DIA3'
          Title.Caption = 'Tercer D'#237'a'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'BLOQUE3'
          Title.Caption = 'Bloque'
          Visible = False
        end>
    end
  end
  object Nueva: TBitBtn
    Left = 8
    Top = 184
    Width = 89
    Height = 25
    HelpContext = 4
    Caption = 'Nueva'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000130B0000130B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF0033333333B333
      333B33FF33337F3333F73BB3777BB7777BB3377FFFF77FFFF77333B000000000
      0B3333777777777777333330FFFFFFFF07333337F33333337F333330FFFFFFFF
      07333337F33333337F333330FFFFFFFF07333337F33333337F333330FFFFFFFF
      07333FF7F33333337FFFBBB0FFFFFFFF0BB37777F3333333777F3BB0FFFFFFFF
      0BBB3777F3333FFF77773330FFFF000003333337F333777773333330FFFF0FF0
      33333337F3337F37F3333330FFFF0F0B33333337F3337F77FF333330FFFF003B
      B3333337FFFF77377FF333B000000333BB33337777777F3377FF3BB3333BB333
      3BB33773333773333773B333333B3333333B7333333733333337}
    NumGlyphs = 2
    TabOrder = 1
    OnClick = NuevaClick
  end
  object Modificar: TBitBtn
    Left = 104
    Top = 184
    Width = 89
    Height = 25
    HelpContext = 5
    Caption = 'Modificar'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333000000
      000033333377777777773333330FFFFFFFF03FF3FF7FF33F3FF700300000FF0F
      00F077F777773F737737E00BFBFB0FFFFFF07773333F7F3333F7E0BFBF000FFF
      F0F077F3337773F3F737E0FBFBFBF0F00FF077F3333FF7F77F37E0BFBF00000B
      0FF077F3337777737337E0FBFBFBFBF0FFF077F33FFFFFF73337E0BF0000000F
      FFF077FF777777733FF7000BFB00B0FF00F07773FF77373377373330000B0FFF
      FFF03337777373333FF7333330B0FFFF00003333373733FF777733330B0FF00F
      0FF03333737F37737F373330B00FFFFF0F033337F77F33337F733309030FFFFF
      00333377737FFFFF773333303300000003333337337777777333}
    NumGlyphs = 2
    TabOrder = 2
    OnClick = ModificarClick
  end
  object Eliminar: TBitBtn
    Left = 200
    Top = 184
    Width = 89
    Height = 25
    HelpContext = 6
    Caption = 'Eliminar'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
      55555FFFFFFF5F55FFF5777777757559995777777775755777F7555555555550
      305555555555FF57F7F555555550055BB0555555555775F777F55555550FB000
      005555555575577777F5555550FB0BF0F05555555755755757F555550FBFBF0F
      B05555557F55557557F555550BFBF0FB005555557F55575577F555500FBFBFB0
      B05555577F555557F7F5550E0BFBFB00B055557575F55577F7F550EEE0BFB0B0
      B05557FF575F5757F7F5000EEE0BFBF0B055777FF575FFF7F7F50000EEE00000
      B0557777FF577777F7F500000E055550805577777F7555575755500000555555
      05555777775555557F5555000555555505555577755555557555}
    NumGlyphs = 2
    TabOrder = 3
    OnClick = EliminarClick
  end
  object Salir: TBitBtn
    Left = 589
    Top = 183
    Width = 75
    Height = 25
    HelpContext = 7
    Caption = 'Salir'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000120B0000120B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00330000000000
      03333377777777777F333301BBBBBBBB033333773F3333337F3333011BBBBBBB
      0333337F73F333337F33330111BBBBBB0333337F373F33337F333301110BBBBB
      0333337F337F33337F333301110BBBBB0333337F337F33337F333301110BBBBB
      0333337F337F33337F333301110BBBBB0333337F337F33337F333301110BBBBB
      0333337F337F33337F333301110BBBBB0333337F337FF3337F33330111B0BBBB
      0333337F337733337F333301110BBBBB0333337F337F33337F333301110BBBBB
      0333337F3F7F33337F333301E10BBBBB0333337F7F7F33337F333301EE0BBBBB
      0333337F777FFFFF7F3333000000000003333377777777777333}
    NumGlyphs = 2
    TabOrder = 4
    OnClick = SalirClick
  end
  object GroupBox2: TGroupBox
    Left = 0
    Top = 223
    Width = 664
    Height = 214
    HelpContext = 8
    Caption = 'Datos de las comisiones'
    TabOrder = 5
    object Label1: TLabel
      Left = 8
      Top = 27
      Width = 86
      Height = 13
      Caption = 'Codigo de Materia'
    end
    object Label2: TLabel
      Left = 9
      Top = 61
      Width = 42
      Height = 13
      Caption = 'Comision'
    end
    object Label3: TLabel
      Left = 192
      Top = 59
      Width = 58
      Height = 13
      Caption = 'Cuatrimestre'
    end
    object Label7: TLabel
      Left = 10
      Top = 88
      Width = 41
      Height = 13
      Caption = 'Docente'
    end
    object NombreMat: TLabel
      Left = 169
      Top = 26
      Width = 3
      Height = 13
    end
    object Labeltitular: TLabel
      Left = 169
      Top = 86
      Width = 3
      Height = 13
    end
    object horario: TStringGrid
      Left = 2
      Top = 131
      Width = 660
      Height = 81
      HelpContext = 9
      TabStop = False
      Align = alBottom
      Color = clBtnFace
      ColCount = 6
      DefaultColWidth = 100
      FixedColor = clSilver
      RowCount = 3
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -12
      Font.Name = 'Arial'
      Font.Style = []
      GridLineWidth = 0
      Options = [goFixedVertLine, goFixedHorzLine, goVertLine, goHorzLine, goThumbTracking]
      ParentFont = False
      ScrollBars = ssNone
      TabOrder = 7
      OnKeyDown = horarioKeyDown
      OnKeyPress = horarioKeyPress
      OnKeyUp = horarioKeyUp
      OnMouseUp = horarioMouseUp
    end
    object Comision: TEdit
      Left = 100
      Top = 51
      Width = 49
      Height = 21
      HelpContext = 12
      MaxLength = 3
      TabOrder = 1
      OnKeyPress = ComisionKeyPress
    end
    object Cuatrimestre: TEdit
      Left = 264
      Top = 51
      Width = 65
      Height = 21
      HelpContext = 13
      MaxLength = 3
      TabOrder = 2
    end
    object GrabaMateria: TBitBtn
      Left = 523
      Top = 16
      Width = 123
      Height = 33
      HelpContext = 14
      Caption = 'Graba Comision'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000120B0000120B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555555555555
        555555555555555555555555555555555555555555FF55555555555559055555
        55555555577FF5555555555599905555555555557777F5555555555599905555
        555555557777FF5555555559999905555555555777777F555555559999990555
        5555557777777FF5555557990599905555555777757777F55555790555599055
        55557775555777FF5555555555599905555555555557777F5555555555559905
        555555555555777FF5555555555559905555555555555777FF55555555555579
        05555555555555777FF5555555555557905555555555555777FF555555555555
        5990555555555555577755555555555555555555555555555555}
      NumGlyphs = 2
      TabOrder = 8
      OnClick = GrabaMateriaClick
    end
    object CancelaGrabacion: TBitBtn
      Left = 522
      Top = 55
      Width = 123
      Height = 34
      HelpContext = 15
      Caption = 'Cancela Grabaci'#243'n'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000130B0000130B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        333333333333333333333333333333333333333FFF33FF333FFF339993370733
        999333777FF37FF377733339993000399933333777F777F77733333399970799
        93333333777F7377733333333999399933333333377737773333333333990993
        3333333333737F73333333333331013333333333333777FF3333333333910193
        333333333337773FF3333333399000993333333337377737FF33333399900099
        93333333773777377FF333399930003999333337773777F777FF339993370733
        9993337773337333777333333333333333333333333333333333333333333333
        3333333333333333333333333333333333333333333333333333}
      NumGlyphs = 2
      TabOrder = 9
      OnClick = CancelaGrabacionClick
    end
    object RadioGroup1: TRadioGroup
      Left = 360
      Top = 68
      Width = 153
      Height = 57
      HelpContext = 18
      Caption = 'Titularidad de la comis'#243'n'
      TabOrder = 4
    end
    object titular: TRadioButton
      Left = 369
      Top = 86
      Width = 113
      Height = 17
      HelpContext = 19
      Caption = 'Titular'
      TabOrder = 5
    end
    object suplente: TRadioButton
      Left = 368
      Top = 105
      Width = 113
      Height = 17
      HelpContext = 20
      Caption = 'Suplente'
      TabOrder = 6
    end
    object ComboMaterias: TComboEdit
      Left = 100
      Top = 22
      Width = 65
      Height = 21
      ClickKey = 112
      GlyphKind = gkEllipsis
      MaxLength = 2
      NumGlyphs = 1
      TabOrder = 0
      OnButtonClick = ComboMateriasButtonClick
      OnExit = ComboMateriasExit
    end
    object ComboDocente: TComboEdit
      Left = 100
      Top = 82
      Width = 65
      Height = 21
      ClickKey = 112
      GlyphKind = gkEllipsis
      MaxLength = 3
      NumGlyphs = 1
      TabOrder = 3
      OnButtonClick = ComboDocenteButtonClick
      OnExit = ComboDocenteExit
    end
  end
  object CertServ: TBitBtn
    Left = 494
    Top = 183
    Width = 89
    Height = 25
    HelpContext = 21
    Caption = 'Certificaci'#243'n'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000130B0000130B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
      33333333333FFFFFFFFF333333000000000033333377777777773333330FFFFF
      FFF03333337F333333373333330FFFFFFFF03333337F3FF3FFF73333330F00F0
      00F03333F37F773777373330330FFFFFFFF03337FF7F3F3FF3F73339030F0800
      F0F033377F7F737737373339900FFFFFFFF03FF7777F3FF3FFF70999990F00F0
      00007777777F7737777709999990FFF0FF0377777777FF37F3730999999908F0
      F033777777777337F73309999990FFF0033377777777FFF77333099999000000
      3333777777777777333333399033333333333337773333333333333903333333
      3333333773333333333333303333333333333337333333333333}
    NumGlyphs = 2
    TabOrder = 6
    Visible = False
    OnClick = CertServClick
  end
  object DataSource1: TDataSource
    DataSet = CustomerData.IbDsVarios
    Left = 496
    Top = 112
  end
  object Timer1: TTimer
    Interval = 500
    OnTimer = FormPaint
    Left = 568
    Top = 240
  end
  object QrPosval: TpFIBQuery
    Left = 384
    Top = 184
  end
end
