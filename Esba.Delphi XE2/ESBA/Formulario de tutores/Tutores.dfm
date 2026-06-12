object FrmTutores: TFrmTutores
  Left = 163
  Top = 157
  BorderStyle = bsDialog
  Caption = 'Tutores del Alumno'
  ClientHeight = 515
  ClientWidth = 643
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object GbGrilla: TGroupBox
    Left = 0
    Top = 0
    Width = 643
    Height = 217
    Align = alTop
    Caption = 'Listado de tutores'
    TabOrder = 0
    object DBGrid1: TDBGrid
      Left = 2
      Top = 15
      Width = 639
      Height = 154
      DataSource = DataSource1
      Options = [dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgRowSelect, dgConfirmDelete, dgCancelOnExit]
      TabOrder = 0
      TitleFont.Charset = DEFAULT_CHARSET
      TitleFont.Color = clWindowText
      TitleFont.Height = -11
      TitleFont.Name = 'MS Sans Serif'
      TitleFont.Style = []
      Columns = <
        item
          Expanded = False
          FieldName = 'FNOMBRE'
          Title.Caption = 'Nombre'
          Width = 200
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'FAPELLIDO'
          Title.Caption = 'Apellido'
          Width = 180
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'FDNI'
          Title.Caption = 'Documento'
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'FDIRECC'
          Title.Caption = 'Direccion'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'FTELEFO'
          Title.Caption = 'Telefono'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'FCELULAR'
          Title.Caption = 'Celular'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'FEMAIL'
          Title.Caption = 'Email'
          Visible = False
        end
        item
          Expanded = False
          FieldName = 'FPARENT'
          Title.Caption = 'Parentezco'
          Width = 90
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'FRETIRA'
          Title.Caption = 'Retira'
          Visible = False
        end>
    end
    object NuevoTutor: TBitBtn
      Left = 9
      Top = 175
      Width = 89
      Height = 33
      Caption = '&Nuevo'
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
      OnClick = NuevoTutorClick
    end
    object modificaTutor: TBitBtn
      Left = 104
      Top = 176
      Width = 89
      Height = 33
      Caption = '&Modifica'
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
      OnClick = modificaTutorClick
    end
    object eliminaTutor: TBitBtn
      Left = 200
      Top = 176
      Width = 89
      Height = 33
      Caption = '&Elimina'
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
      OnClick = eliminaTutorClick
    end
    object salir: TBitBtn
      Left = 544
      Top = 176
      Width = 89
      Height = 33
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
      OnClick = salirClick
    end
  end
  object GroupBox1: TGroupBox
    Left = 0
    Top = 218
    Width = 641
    Height = 295
    Margins.Top = 0
    TabOrder = 1
    object GrabaPermiso: TBitBtn
      Left = 412
      Top = 255
      Width = 107
      Height = 34
      Caption = 'Graba'
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
      TabOrder = 0
      OnClick = GrabaPermisoClick
    end
    object CancelaGrabacion: TBitBtn
      Left = 530
      Top = 255
      Width = 108
      Height = 34
      Caption = 'Cancela'
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
      TabOrder = 1
      OnClick = CancelaGrabacionClick
    end
    object GroupBox2: TGroupBox
      Left = 3
      Top = 5
      Width = 637
      Height = 84
      Caption = 'Datos Personales'
      TabOrder = 2
      object Label1: TLabel
        Left = 5
        Top = 23
        Width = 37
        Height = 13
        Caption = 'Nombre'
      end
      object Label2: TLabel
        Left = 5
        Top = 50
        Width = 37
        Height = 13
        Caption = 'Apellido'
      end
      object Label3: TLabel
        Left = 240
        Top = 49
        Width = 55
        Height = 13
        Caption = 'Documento'
      end
      object Label8: TLabel
        Left = 240
        Top = 22
        Width = 54
        Height = 13
        Caption = 'Parentesco'
      end
      object Dbnombre: TDBEdit
        Left = 62
        Top = 20
        Width = 164
        Height = 21
        DataField = 'FNOMBRE'
        DataSource = DataSource1
        TabOrder = 0
      end
      object DBApellido: TDBEdit
        Left = 62
        Top = 47
        Width = 164
        Height = 21
        DataField = 'FAPELLIDO'
        DataSource = DataSource1
        TabOrder = 1
      end
      object DBdni: TDBEdit
        Left = 300
        Top = 46
        Width = 125
        Height = 21
        DataField = 'FDNI'
        DataSource = DataSource1
        TabOrder = 2
      end
      object DBCparentesco: TDBComboBox
        Left = 300
        Top = 19
        Width = 125
        Height = 21
        Style = csDropDownList
        DataField = 'FPARENT'
        DataSource = DataSource1
        Items.Strings = (
          'Padre'
          'Madre'
          'Tio'
          'Hermano'
          'Abuelo/a'
          'Tutor')
        TabOrder = 3
      end
      object DBretira: TDBCheckBox
        Left = 486
        Top = 19
        Width = 97
        Height = 17
        Caption = 'Puede Retirar'
        DataField = 'FRETIRA'
        DataSource = DataSource1
        TabOrder = 4
        ValueChecked = 'S'
        ValueUnchecked = 'N'
        OnClick = DBretiraClick
      end
      object DBCheckBox1: TDBCheckBox
        Left = 486
        Top = 35
        Width = 148
        Height = 17
        Caption = 'Responsable Economico'
        DataField = 'FRESPON'
        DataSource = DataSource1
        TabOrder = 5
        ValueChecked = 'S'
        ValueUnchecked = 'N'
        OnClick = DBretiraClick
      end
      object dbAccesoWeb: TDBCheckBox
        Left = 486
        Top = 51
        Width = 148
        Height = 17
        Caption = 'AccesoWeb'
        DataField = 'FUSUWEB'
        DataSource = DataSource1
        TabOrder = 6
        ValueChecked = 'S'
        ValueUnchecked = 'N'
        OnClick = DBretiraClick
      end
    end
    object GroupBox3: TGroupBox
      Left = 3
      Top = 91
      Width = 637
      Height = 76
      Caption = 'Datos de Ubicacion'
      TabOrder = 3
      object Label4: TLabel
        Left = 3
        Top = 20
        Width = 45
        Height = 13
        Caption = 'Direccion'
      end
      object Label9: TLabel
        Left = 3
        Top = 50
        Width = 46
        Height = 13
        Caption = 'Localidad'
      end
      object Label10: TLabel
        Left = 312
        Top = 20
        Width = 54
        Height = 13
        Caption = 'Cod. Postal'
      end
      object DBdireccion: TDBEdit
        Left = 62
        Top = 17
        Width = 217
        Height = 21
        DataField = 'FDIRECC'
        DataSource = DataSource1
        TabOrder = 0
      end
      object DBlocali: TDBEdit
        Left = 62
        Top = 47
        Width = 149
        Height = 21
        DataField = 'FLOCALI'
        DataSource = DataSource1
        TabOrder = 1
      end
      object DBcodpos: TDBEdit
        Left = 377
        Top = 17
        Width = 63
        Height = 21
        DataField = 'FCODPOS'
        DataSource = DataSource1
        TabOrder = 2
      end
    end
    object GroupBox4: TGroupBox
      Left = 3
      Top = 167
      Width = 637
      Height = 82
      Caption = 'Informacion de contacto'
      TabOrder = 4
      object Label7: TLabel
        Left = 5
        Top = 23
        Width = 26
        Height = 13
        Caption = 'EMail'
      end
      object Label5: TLabel
        Left = 5
        Top = 48
        Width = 42
        Height = 13
        Caption = 'Telefono'
      end
      object Label6: TLabel
        Left = 203
        Top = 48
        Width = 32
        Height = 13
        Caption = 'Celular'
      end
      object DBTelefono: TDBEdit
        Left = 62
        Top = 45
        Width = 129
        Height = 21
        DataField = 'FTELEFO'
        DataSource = DataSource1
        TabOrder = 0
      end
      object DBMail: TDBEdit
        Left = 62
        Top = 14
        Width = 378
        Height = 21
        DataField = 'FEMAIL'
        DataSource = DataSource1
        TabOrder = 1
      end
      object DBCelular: TDBEdit
        Left = 244
        Top = 45
        Width = 148
        Height = 21
        DataField = 'FCELULAR'
        DataSource = DataSource1
        TabOrder = 2
      end
    end
  end
  object DataSource1: TDataSource
    DataSet = MtTutores
    Left = 464
    Top = 104
  end
  object MtTutores: TkbmMemTable
    DesignActivation = True
    AttachedAutoRefresh = True
    AttachMaxCount = 1
    FieldDefs = <
      item
        Name = 'FNOMBRE'
        DataType = ftString
        Size = 60
      end
      item
        Name = 'FAPELLIDO'
        DataType = ftString
        Size = 60
      end
      item
        Name = 'FDNI'
        DataType = ftInteger
      end
      item
        Name = 'FDIRECC'
        DataType = ftString
        Size = 100
      end
      item
        Name = 'FTELEFO'
        DataType = ftString
        Size = 30
      end
      item
        Name = 'FCELULAR'
        DataType = ftString
        Size = 30
      end
      item
        Name = 'FEMAIL'
        DataType = ftString
        Size = 100
      end
      item
        Name = 'FPARENT'
        DataType = ftString
        Size = 30
      end
      item
        Name = 'FRETIRA'
        DataType = ftString
        Size = 1
      end>
    IndexDefs = <>
    SortOptions = []
    PersistentBackup = False
    ProgressFlags = [mtpcLoad, mtpcSave, mtpcCopy]
    LoadedCompletely = False
    SavedCompletely = False
    FilterOptions = []
    Version = '7.15.00 Professional Edition'
    LanguageID = 0
    SortID = 0
    SubLanguageID = 1
    LocaleID = 1024
    Left = 424
    Top = 104
    object MtTutoresFNOMBRE: TStringField
      FieldName = 'FNOMBRE'
      Size = 60
    end
    object MtTutoresFAPELLIDO: TStringField
      FieldName = 'FAPELLIDO'
      Size = 60
    end
    object MtTutoresFDNI: TIntegerField
      FieldName = 'FDNI'
    end
    object MtTutoresFDIRECC: TStringField
      FieldName = 'FDIRECC'
      Size = 100
    end
    object MtTutoresFTELEFO: TStringField
      FieldName = 'FTELEFO'
      Size = 30
    end
    object MtTutoresFCELULAR: TStringField
      FieldName = 'FCELULAR'
      Size = 30
    end
    object MtTutoresFEMAIL: TStringField
      FieldName = 'FEMAIL'
      Size = 100
    end
    object MtTutoresFPARENT: TStringField
      FieldName = 'FPARENT'
      Size = 30
    end
    object MtTutoresFRETIRA: TStringField
      FieldName = 'FRETIRA'
      Size = 1
    end
    object MtTutoresFLOCALI: TStringField
      FieldName = 'FLOCALI'
      Size = 30
    end
    object MtTutoresFCODPOS: TStringField
      FieldName = 'FCODPOS'
      Size = 10
    end
    object MtTutoresFRESPON: TStringField
      FieldName = 'FRESPON'
      Size = 1
    end
    object MtTutoresFUSUWEB: TStringField
      FieldName = 'FUSUWEB'
      Size = 1
    end
  end
end
