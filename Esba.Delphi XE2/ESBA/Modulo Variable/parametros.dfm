object FrmParametros: TFrmParametros
  Left = 899
  Top = 299
  BorderIcons = [biSystemMenu]
  BorderStyle = bsDialog
  Caption = 'Parametros'
  ClientHeight = 266
  ClientWidth = 279
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -13
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnActivate = FormActivate
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 16
  object BtnAceptar: TButton
    Left = 134
    Top = 233
    Width = 65
    Height = 25
    Caption = '&Aceptar'
    TabOrder = 0
    OnClick = BtnAceptarClick
  end
  object BtnCancelar: TButton
    Left = 206
    Top = 233
    Width = 65
    Height = 25
    Caption = '&Cancelar'
    TabOrder = 1
    OnClick = BtnCancelarClick
  end
end
