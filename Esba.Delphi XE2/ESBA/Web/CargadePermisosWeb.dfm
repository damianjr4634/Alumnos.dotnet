object FrmCargaPermisosWeb: TFrmCargaPermisosWeb
  Left = 312
  Top = 149
  Caption = 'Permisos cargados via Web'
  ClientHeight = 448
  ClientWidth = 1184
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Panel1: TPanel
    Left = 0
    Top = 0
    Width = 1184
    Height = 405
    Align = alTop
    TabOrder = 0
    object Grilla: TDBGrid
      Left = 1
      Top = 1
      Width = 1182
      Height = 403
      Align = alClient
      DataSource = DataSource1
      Options = [dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgMultiSelect, dgTitleClick, dgTitleHotTrack]
      TabOrder = 0
      TitleFont.Charset = DEFAULT_CHARSET
      TitleFont.Color = clWindowText
      TitleFont.Height = -11
      TitleFont.Name = 'MS Sans Serif'
      TitleFont.Style = []
      OnColExit = GrillaColExit
      OnEditButtonClick = GrillaEditButtonClick
      OnKeyUp = GrillaKeyUp
      OnTitleClick = GrillaTitleClick
      Columns = <
        item
          ButtonStyle = cbsEllipsis
          Expanded = False
          FieldName = 'COD_ALU'
          Title.Caption = 'Alumno'
          Width = 85
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'NOMBRE'
          ReadOnly = True
          Title.Caption = 'Nombre'
          Width = 270
          Visible = True
        end
        item
          ButtonStyle = cbsEllipsis
          Expanded = False
          FieldName = 'COD_MAT'
          Title.Caption = 'Materia'
          Width = 55
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'MATERIA'
          ReadOnly = True
          Title.Caption = 'Descripcion'
          Width = 200
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'CUTUCO'
          Title.Caption = 'Cuatrimestre'
          Visible = True
        end
        item
          ButtonStyle = cbsEllipsis
          Expanded = False
          FieldName = 'MESA'
          Title.Caption = 'Mesa'
          Width = 45
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'FECHA'
          ReadOnly = True
          Title.Caption = 'Fecha'
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'NUMPER'
          Title.Caption = 'Permiso Numero'
          Visible = True
        end
        item
          Expanded = False
          FieldName = 'FERRMSG'
          PickList.Strings = (
            'Si'
            'No')
          Title.Caption = 'Problemas'
          Width = 300
          Visible = True
        end>
    end
  end
  object Panel2: TPanel
    Left = 0
    Top = 403
    Width = 1184
    Height = 45
    Align = alBottom
    TabOrder = 1
    object Grabamesa: TBitBtn
      Left = 1005
      Top = 1
      Width = 89
      Height = 43
      Align = alRight
      Caption = '&Grabar'
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
      OnClick = GrabamesaClick
    end
    object CancelaGrabacion: TBitBtn
      Left = 1094
      Top = 1
      Width = 89
      Height = 43
      Align = alRight
      Caption = '&Salir'
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
  end
  object DataSource1: TDataSource
    DataSet = MtPermisos
    Left = 64
    Top = 240
  end
  object MtPermisos: TkbmMemTable
    DesignActivation = True
    AttachedAutoRefresh = True
    AttachMaxCount = 1
    FieldDefs = <
      item
        Name = 'COD_ALU'
        Attributes = [faRequired]
        DataType = ftString
        Size = 11
      end
      item
        Name = 'NOMBRE'
        DataType = ftString
        Size = 80
      end
      item
        Name = 'COD_MAT'
        Attributes = [faRequired]
        DataType = ftString
        Size = 2
      end
      item
        Name = 'MATERIA'
        DataType = ftString
        Size = 80
      end
      item
        Name = 'CUTUCO'
        DataType = ftInteger
      end
      item
        Name = 'FECHA'
        DataType = ftDate
      end
      item
        Name = 'NUMPER'
        DataType = ftInteger
      end>
    IndexDefs = <
      item
        Name = 'COD_ALU'
        Fields = 'COD_ALU'
      end
      item
        Name = 'COD_ALU_DESC'
        Fields = 'COD_ALU'
        Options = [ixDescending]
      end
      item
        Name = 'NOMBRE'
        Fields = 'NOMBRE'
      end
      item
        Name = 'NOMBRE_DESC'
        Fields = 'NOMBRE'
        Options = [ixDescending]
      end
      item
        Name = 'MATERIA'
        Fields = 'MATERIA'
      end
      item
        Name = 'MATERIA_DESC'
        Fields = 'MATERIA'
        Options = [ixDescending]
      end
      item
        Name = 'COD_MAT'
        Fields = 'COD_MAT'
      end
      item
        Name = 'COD_MAT_DESC'
        Fields = 'COD_MAT'
        Options = [ixDescending]
      end
      item
        Name = 'CUTUCO'
        Fields = 'CUTUCO'
      end
      item
        Name = 'CUTUCO_DESC'
        Fields = 'CUTUCO'
        Options = [ixDescending]
      end
      item
        Name = 'MESA'
        Fields = 'MESA'
      end
      item
        Name = 'MESA_DESC'
        Fields = 'MESA'
        Options = [ixDescending]
      end
      item
        Name = 'FECHA'
        Fields = 'FECHA'
      end
      item
        Name = 'FECHA_DESC'
        Fields = 'FECHA'
        Options = [ixDescending]
      end>
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
    Left = 224
    Top = 224
    object MtPermisosCOD_ALU: TStringField
      FieldName = 'COD_ALU'
      Required = True
      OnSetText = MtPermisosSetText
      Size = 11
    end
    object MtPermisosNOMBRE: TStringField
      FieldName = 'NOMBRE'
      Size = 80
    end
    object MtPermisosCOD_MAT: TStringField
      FieldName = 'COD_MAT'
      Required = True
      OnSetText = MtPermisosSetText
      Size = 2
    end
    object MtPermisosMATERIA: TStringField
      FieldName = 'MATERIA'
      Size = 80
    end
    object MtPermisosCUTUCO: TIntegerField
      FieldName = 'CUTUCO'
    end
    object MtPermisosFECHA: TDateField
      FieldName = 'FECHA'
    end
    object MtPermisosNUMPER: TIntegerField
      FieldName = 'NUMPER'
    end
    object MtPermisosMESA: TIntegerField
      FieldName = 'MESA'
    end
    object MtPermisosFERRMSG: TStringField
      FieldName = 'FERRMSG'
      Size = 1000
    end
  end
  object IbupIns: TpFIBQuery
    Transaction = TrPermisos
    Database = CustomerData.FBase
    Left = 104
    Top = 329
  end
  object TrPermisos: TpFIBTransaction
    DefaultDatabase = CustomerData.FBase
    TimeoutAction = TARollback
    Left = 184
    Top = 313
  end
end
