object Frmenviomail: TFrmenviomail
  Left = 423
  Top = 29
  BorderStyle = bsDialog
  Caption = 'Envio de correo electronico por comisi'#243'n'
  ClientHeight = 698
  ClientWidth = 585
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnActivate = FormActivate
  PixelsPerInch = 96
  TextHeight = 13
  object Label4: TLabel
    Left = 0
    Top = 0
    Width = 585
    Height = 13
    Align = alTop
    ExplicitWidth = 3
  end
  object GroupBox1: TGroupBox
    Left = 3
    Top = 55
    Width = 278
    Height = 213
    Caption = 'Alumnos con E-Mail'
    TabOrder = 0
    object Conmail: TListBox
      Left = 2
      Top = 15
      Width = 274
      Height = 196
      Align = alClient
      DragMode = dmAutomatic
      ItemHeight = 13
      MultiSelect = True
      Sorted = True
      TabOrder = 0
      OnDragDrop = MailDragDrop
      OnDragOver = MailDragOver
    end
  end
  object GroupBox2: TGroupBox
    Left = 3
    Top = 268
    Width = 278
    Height = 125
    Caption = 'Alumnos sin E-Mail'
    TabOrder = 1
    object Sinmail: TListBox
      Left = 2
      Top = 15
      Width = 274
      Height = 108
      Align = alClient
      ItemHeight = 13
      TabOrder = 0
    end
  end
  object GroupBox3: TGroupBox
    Left = 287
    Top = 56
    Width = 294
    Height = 113
    Caption = 'Para'
    TabOrder = 2
    object Toponeuno: TSpeedButton
      Left = 3
      Top = 16
      Width = 23
      Height = 22
      Hint = 'Pasa uno'
      Caption = '>'
      Flat = True
      OnClick = poneunoClick
    end
    object Toponetodos: TSpeedButton
      Left = 3
      Top = 38
      Width = 23
      Height = 22
      Hint = 'Pasan todos'
      Caption = '>>'
      Flat = True
      OnClick = ponetodosClick
    end
    object Tosacauno: TSpeedButton
      Left = 3
      Top = 60
      Width = 23
      Height = 22
      Hint = 'Saca uno'
      Caption = '<'
      Flat = True
      OnClick = sacaunoClick
    end
    object Tosacatodos: TSpeedButton
      Left = 3
      Top = 82
      Width = 23
      Height = 24
      Hint = 'Sacan todos'
      Caption = '<<'
      Flat = True
      OnClick = sacatodosClick
    end
    object Lbpara: TListBox
      Left = 28
      Top = 16
      Width = 256
      Height = 89
      DragMode = dmAutomatic
      ItemHeight = 13
      MultiSelect = True
      Sorted = True
      TabOrder = 0
      OnDragDrop = MailDragDrop
      OnDragOver = MailDragOver
    end
  end
  object GroupBox4: TGroupBox
    Left = 0
    Top = 13
    Width = 585
    Height = 43
    Align = alTop
    Caption = 'Comisi'#243'n'
    TabOrder = 3
    object Label1: TLabel
      Left = 9
      Top = 20
      Width = 96
      Height = 13
      Caption = 'N'#250'mero de comisi'#243'n'
    end
    object Label2: TLabel
      Left = 222
      Top = 19
      Width = 90
      Height = 13
      Caption = 'Cuatrimestre actual'
    end
    object comision: TEdit
      Left = 118
      Top = 14
      Width = 69
      Height = 21
      MaxLength = 3
      TabOrder = 0
    end
    object buscar: TBitBtn
      Left = 498
      Top = 14
      Width = 75
      Height = 25
      Caption = '&Buscar'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000130B0000130B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        333333333333333333FF33333333333330003FF3FFFFF3333777003000003333
        300077F777773F333777E00BFBFB033333337773333F7F33333FE0BFBF000333
        330077F3337773F33377E0FBFBFBF033330077F3333FF7FFF377E0BFBF000000
        333377F3337777773F3FE0FBFBFBFBFB039977F33FFFFFFF7377E0BF00000000
        339977FF777777773377000BFB03333333337773FF733333333F333000333333
        3300333777333333337733333333333333003333333333333377333333333333
        333333333333333333FF33333333333330003333333333333777333333333333
        3000333333333333377733333333333333333333333333333333}
      NumGlyphs = 2
      TabOrder = 1
      OnClick = buscarClick
    end
    object TxtCuat: TEdit
      Left = 318
      Top = 14
      Width = 52
      Height = 21
      MaxLength = 3
      TabOrder = 2
    end
  end
  object GroupBox5: TGroupBox
    Left = 287
    Top = 168
    Width = 294
    Height = 113
    Caption = 'Copia (CC)'
    TabOrder = 4
    object CCPoneUno: TSpeedButton
      Left = 3
      Top = 16
      Width = 23
      Height = 22
      Hint = 'Pasa uno'
      Caption = '>'
      Flat = True
      OnClick = poneunoClick
    end
    object CCPoneTodos: TSpeedButton
      Left = 3
      Top = 38
      Width = 23
      Height = 22
      Hint = 'Pasan todos'
      Caption = '>>'
      Flat = True
      OnClick = ponetodosClick
    end
    object CCSacaUno: TSpeedButton
      Left = 3
      Top = 60
      Width = 23
      Height = 22
      Hint = 'Saca uno'
      Caption = '<'
      Flat = True
      OnClick = sacaunoClick
    end
    object CCSacaTodos: TSpeedButton
      Left = 3
      Top = 82
      Width = 23
      Height = 24
      Hint = 'Sacan todos'
      Caption = '<<'
      Flat = True
      OnClick = sacatodosClick
    end
    object LbCC: TListBox
      Left = 32
      Top = 16
      Width = 256
      Height = 89
      DragMode = dmAutomatic
      ItemHeight = 13
      MultiSelect = True
      Sorted = True
      TabOrder = 0
      OnDragDrop = MailDragDrop
      OnDragOver = MailDragOver
    end
  end
  object GroupBox6: TGroupBox
    Left = 287
    Top = 280
    Width = 296
    Height = 113
    Caption = 'Copia Oculta (CCO)'
    TabOrder = 5
    object CCOSacaTodos: TSpeedButton
      Left = 3
      Top = 80
      Width = 23
      Height = 24
      Hint = 'Sacan todos'
      Caption = '<<'
      Flat = True
      OnClick = sacatodosClick
    end
    object CCOSacaUno: TSpeedButton
      Left = 3
      Top = 60
      Width = 23
      Height = 22
      Hint = 'Saca uno'
      Caption = '<'
      Flat = True
      OnClick = sacaunoClick
    end
    object CCOPoneTodos: TSpeedButton
      Left = 3
      Top = 38
      Width = 23
      Height = 22
      Hint = 'Pasan todos'
      Caption = '>>'
      Flat = True
      OnClick = ponetodosClick
    end
    object CCOPoneuno: TSpeedButton
      Left = 3
      Top = 16
      Width = 23
      Height = 22
      Hint = 'Pasa uno'
      Caption = '>'
      Flat = True
      OnClick = poneunoClick
    end
    object LbCCO: TListBox
      Left = 27
      Top = 16
      Width = 256
      Height = 89
      DragMode = dmAutomatic
      ItemHeight = 13
      MultiSelect = True
      Sorted = True
      TabOrder = 0
      OnDragDrop = MailDragDrop
      OnDragOver = MailDragOver
    end
  end
  object GroupBox7: TGroupBox
    Left = 0
    Top = 659
    Width = 585
    Height = 39
    Align = alBottom
    TabOrder = 6
    ExplicitTop = 688
    object Configurar: TBitBtn
      Left = 333
      Top = 12
      Width = 75
      Height = 25
      Caption = '&Configurar'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000120B0000120B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00555550FF0559
        1950555FF75F7557F7F757000FF055591903557775F75557F77570FFFF055559
        1933575FF57F5557F7FF0F00FF05555919337F775F7F5557F7F700550F055559
        193577557F7F55F7577F07550F0555999995755575755F7FFF7F5570F0755011
        11155557F755F777777555000755033305555577755F75F77F55555555503335
        0555555FF5F75F757F5555005503335505555577FF75F7557F55505050333555
        05555757F75F75557F5505000333555505557F777FF755557F55000000355557
        07557777777F55557F5555000005555707555577777FF5557F55553000075557
        0755557F7777FFF5755555335000005555555577577777555555}
      NumGlyphs = 2
      TabOrder = 0
      OnClick = ConfigurarClick
    end
    object enviar: TBitBtn
      Left = 418
      Top = 12
      Width = 75
      Height = 25
      Caption = '&Enviar'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000120B0000120B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        3333333333333333333333333333333333333FFFFFFFFFFFFFFF000000000000
        000077777777777777770FFFFFFFFFFFFFF07F3333FFF33333370FFFF777FFFF
        FFF07F333777333333370FFFFFFFFFFFFFF07F3333FFFFFF33370FFFF777777F
        FFF07F33377777733FF70FFFFFFFFFFF99907F3FFF33333377770F777FFFFFFF
        9CA07F77733333337F370FFFFFFFFFFF9A907FFFFFFFFFFF7FF7000000000000
        0000777777777777777733333333333333333333333333333333333333333333
        3333333333333333333333333333333333333333333333333333333333333333
        3333333333333333333333333333333333333333333333333333}
      NumGlyphs = 2
      TabOrder = 1
      OnClick = enviarClick
    end
    object salir: TBitBtn
      Left = 506
      Top = 12
      Width = 75
      Height = 25
      Caption = '&Salir'
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
      TabOrder = 2
      OnClick = salirClick
    end
    object Firma: TCheckBox
      Left = 7
      Top = 16
      Width = 97
      Height = 17
      Caption = 'Con firma digital'
      TabOrder = 3
    end
  end
  object Mesaje: TGroupBox
    Left = 0
    Top = 531
    Width = 585
    Height = 128
    Align = alBottom
    Caption = 'Mensaje'
    TabOrder = 10
    ExplicitTop = 560
    object Mensaje: TMemo
      Left = 2
      Top = 15
      Width = 581
      Height = 111
      Align = alClient
      TabOrder = 0
      ExplicitWidth = 582
      ExplicitHeight = 136
    end
  end
  object GroupBox8: TGroupBox
    Left = 359
    Top = 395
    Width = 225
    Height = 95
    Caption = 'Adjuntos'
    TabOrder = 7
    object ArchAdj: TListBox
      Left = 2
      Top = 15
      Width = 221
      Height = 50
      Align = alTop
      ItemHeight = 13
      TabOrder = 0
      OnKeyUp = ArchAdjKeyUp
    end
    object Examinar: TBitBtn
      Left = 146
      Top = 65
      Width = 75
      Height = 25
      Caption = '&Agregar'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000120B0000120B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00300000000000
        0000377777777777777707FFFFFFFFFFFF70773FF33333333F770F77FFFFFFFF
        77F07F773FFFFFFF77F70FFF7700000000007F337777777777770FFFFF0BBBBB
        BBB07F333F7F3FF33FF70FFF700B00BB00B07F3F777F77F377370F707F0BB0B0
        0BB07F77337F37F77337007EEE0BB0B0BBB077FFFF7F37F7F3370777770EE000
        EEE07777777F3777F3F7307EEE0E0E00E0E03773FF7F7377F73733707F0EE000
        0EE03337737F377773373333700EEE00EEE03333377F3377FF373333330EEEE0
        0EE03333337F33377F373333330EEEE00EE03333337F333773373333330EEEEE
        EEE03333337FFFFFFFF733333300000000003333337777777777}
      NumGlyphs = 2
      TabOrder = 1
      OnClick = ExaminarClick
    end
    object BtnQuitar: TBitBtn
      Left = 9
      Top = 67
      Width = 75
      Height = 25
      Caption = '&Quitar'
      Glyph.Data = {
        76010000424D7601000000000000760000002800000020000000100000000100
        04000000000000010000130B0000130B00001000000000000000000000000000
        800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        333333333333333333FF33333333333330003333333333333777333333333333
        300033FFFFFF3333377739999993333333333777777F3333333F399999933333
        3300377777733333337733333333333333003333333333333377333333333333
        3333333333333333333F333333333333330033333F33333333773333C3333333
        330033337F3333333377333CC3333333333333F77FFFFFFF3FF33CCCCCCCCCC3
        993337777777777F77F33CCCCCCCCCC399333777777777737733333CC3333333
        333333377F33333333FF3333C333333330003333733333333777333333333333
        3000333333333333377733333333333333333333333333333333}
      NumGlyphs = 2
      TabOrder = 2
      OnClick = BtnQuitarClick
    end
  end
  object GroupBox9: TGroupBox
    Left = 0
    Top = 395
    Width = 353
    Height = 95
    Caption = 'Direcciones Extras'
    TabOrder = 8
    object Label3: TLabel
      Left = 8
      Top = 21
      Width = 22
      Height = 13
      Caption = 'Para'
    end
    object Label5: TLabel
      Left = 8
      Top = 45
      Width = 14
      Height = 13
      Caption = 'CC'
    end
    object Label6: TLabel
      Left = 8
      Top = 70
      Width = 22
      Height = 13
      Caption = 'CCO'
    end
    object DEPara: TEdit
      Left = 35
      Top = 14
      Width = 314
      Height = 21
      TabOrder = 0
      OnChange = DEParaChange
    end
    object DECc: TEdit
      Left = 35
      Top = 41
      Width = 314
      Height = 21
      TabOrder = 1
    end
    object DECco: TEdit
      Left = 35
      Top = 67
      Width = 314
      Height = 21
      TabOrder = 2
    end
  end
  object GroupBox10: TGroupBox
    Left = 0
    Top = 489
    Width = 585
    Height = 42
    Align = alBottom
    Caption = 'Asunto'
    TabOrder = 9
    ExplicitTop = 496
    object Asunto: TEdit
      Left = 3
      Top = 15
      Width = 580
      Height = 21
      TabOrder = 0
    end
  end
  object DlgAbrir: TOpenDialog
    Left = 232
    Top = 247
  end
  object IdSMTP: TIdSMTP
    AuthType = satSASL
    SASLMechanisms = <>
    Left = 376
    Top = 583
  end
  object IdSSLIOHandlerSocketOpenSSL1: TIdSSLIOHandlerSocketOpenSSL
    Destination = ':25'
    MaxLineAction = maException
    Port = 25
    DefaultPort = 0
    SSLOptions.Method = sslvSSLv23
    SSLOptions.SSLVersions = [sslvSSLv2, sslvSSLv3, sslvTLSv1]
    SSLOptions.Mode = sslmUnassigned
    SSLOptions.VerifyMode = []
    SSLOptions.VerifyDepth = 0
    Left = 264
    Top = 568
  end
end
