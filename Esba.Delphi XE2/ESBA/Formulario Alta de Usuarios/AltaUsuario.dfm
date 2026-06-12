object FrmAltaUsuario: TFrmAltaUsuario
  Left = 565
  Top = 192
  BorderStyle = bsDialog
  Caption = 'Alta de usuarios'
  ClientHeight = 266
  ClientWidth = 439
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  PixelsPerInch = 96
  TextHeight = 13
  object Graba: TBitBtn
    Left = 352
    Top = 200
    Width = 75
    Height = 25
    Caption = 'Graba'
    Glyph.Data = {
      76010000424D7601000000000000760000002800000020000000100000000100
      04000000000000010000130B0000130B00001000000000000000000000000000
      800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
      FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333330070
      7700333333337777777733333333008088003333333377F73377333333330088
      88003333333377FFFF7733333333000000003FFFFFFF77777777000000000000
      000077777777777777770FFFFFFF0FFFFFF07F3333337F3333370FFFFFFF0FFF
      FFF07F3FF3FF7FFFFFF70F00F0080CCC9CC07F773773777777770FFFFFFFF039
      99337F3FFFF3F7F777F30F0000F0F09999937F7777373777777F0FFFFFFFF999
      99997F3FF3FFF77777770F00F000003999337F773777773777F30FFFF0FF0339
      99337F3FF7F3733777F30F08F0F0337999337F7737F73F7777330FFFF0039999
      93337FFFF7737777733300000033333333337777773333333333}
    NumGlyphs = 2
    TabOrder = 2
    OnClick = GrabaClick
  end
  object Salir: TBitBtn
    Left = 352
    Top = 232
    Width = 75
    Height = 25
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
    TabOrder = 3
    OnClick = SalirClick
  end
  object GroupBox1: TGroupBox
    Left = 8
    Top = 8
    Width = 321
    Height = 121
    Caption = 'Datos del usuario'
    TabOrder = 0
    object Label1: TLabel
      Left = 8
      Top = 24
      Width = 89
      Height = 13
      Caption = 'Nombre de usuario'
    end
    object Label2: TLabel
      Left = 8
      Top = 56
      Width = 54
      Height = 13
      Caption = 'Contrase'#241'a'
    end
    object Label3: TLabel
      Left = 8
      Top = 88
      Width = 91
      Height = 13
      Caption = 'Reitere Contrase'#241'a'
    end
    object RPassword: TEdit
      Left = 112
      Top = 80
      Width = 177
      Height = 21
      MaxLength = 5
      PasswordChar = '*'
      TabOrder = 2
    end
    object Password: TEdit
      Left = 112
      Top = 48
      Width = 177
      Height = 21
      MaxLength = 5
      PasswordChar = '*'
      TabOrder = 1
    end
    object Usuario: TEdit
      Left = 112
      Top = 16
      Width = 177
      Height = 21
      MaxLength = 15
      TabOrder = 0
    end
  end
  object GroupBox2: TGroupBox
    Left = 8
    Top = 136
    Width = 321
    Height = 121
    Caption = 'Datos personales'
    TabOrder = 1
    object Label4: TLabel
      Left = 8
      Top = 24
      Width = 37
      Height = 13
      Caption = 'Nombre'
    end
    object Label5: TLabel
      Left = 8
      Top = 56
      Width = 37
      Height = 13
      Caption = 'Apellido'
    end
    object Label6: TLabel
      Left = 8
      Top = 88
      Width = 28
      Height = 13
      Caption = 'Cargo'
    end
    object Cargo: TEdit
      Left = 112
      Top = 80
      Width = 177
      Height = 21
      MaxLength = 15
      TabOrder = 2
    end
    object Apellido: TEdit
      Left = 112
      Top = 48
      Width = 177
      Height = 21
      MaxLength = 15
      TabOrder = 1
    end
    object Nombre: TEdit
      Left = 112
      Top = 16
      Width = 177
      Height = 21
      MaxLength = 15
      TabOrder = 0
    end
  end
end
