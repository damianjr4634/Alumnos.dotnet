unit sesion;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, FuncionesText , FuncionesSystem, DataModule, CambioPassword;

type
  Tiniciodesesion = class(TForm)
    nombre: TEdit;
    password: TEdit;
    Label1: TLabel;
    Label2: TLabel;
    ok: TBitBtn;
    BitBtn2: TBitBtn;
    StaticText1: TStaticText;
    procedure okClick(Sender: TObject);
    procedure BitBtn2Click(Sender: TObject);
    procedure passwordKeyPress(Sender: TObject; var Key: Char);
    procedure FormActivate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  iniciodesesion: Tiniciodesesion;
  Pasa, SUPERV : Boolean;
  codusu : Integer;
  fecha: AnsiString;

implementation

{$R *.DFM}
uses
  FuncionesConfiguracion;

procedure Tiniciodesesion.FormActivate(Sender: TObject);
begin
      Fecha:='12/12/2013';
      StaticText1.Caption:='';//'La licencia del sistema caduca el '+sesion.fecha;
      Nombre.Text := GetUserName;
      password.SetFocus;
end;

procedure Tiniciodesesion.okClick(Sender: TObject);
begin
 {if StrToDate(Fecha)<date then begin
   MessageDlg('El sistema caduco su licencia. Gracias.',mtError,[mbok],0,mbok);
   exit;
 end;       }
 Pasa:=True;
 CustomerData.Connect;
 CustomerData.FDSUsuarios.ParamByName('NOM').AsString := Nombre.Text;
 CustomerData.FDSUsuarios.Active := True;
 CustomerData.FDSUsuarios.First;
 If (CustomerData.FDSUsuarios.FieldByName('NOMBRE').AsString <> '') Then begin
    If EncriptoCadena2(CustomerData.FDSUsuarios.FieldByName('PASSWD').AsString,-1) <> password.Text Then begin
        MessageDlg('Nombre de usuario o password incorrecto',mtError,[mbok],0,mbok);
        Pasa:=False;
    end
    else begin
        Pasa:=True;
        codusu := customerdata.FDSUsuarios.FieldByname('CODUSU').Value;
        if (customerdata.FDSUsuarios.FieldByname('SUPERV').AsString = 'S') then
          SUPERV := true
        else
          SUPERV := false;
    end
 end else begin
     MessageDlg('Nombre de usuario o password incorrecto',mtError,[mbok],0,mbok);
     Pasa:=False;
 end;
 if pasa then begin
     if (UpperCase(customerdata.FDSUsuarios.FieldByname('CAMPASS').Value)='S') then begin
         FrmCambioPass := TFrmCambioPass.Create(Self);
         CambioPassword.VCodUsu := codusu;
         FrmCambioPass.Position:=poScreenCenter;
         FrmCambioPass.Salir.Enabled:=False;
         FrmCambioPass.ShowModal;
         FrmCambioPass.Free;
     end;
     FuncionesConfiguracion.CodUsu:= codusu;
     CustomerData.FDSUsuarios.Active := False;
     CustomerData.DisConnect;
     Close;
 end;
end;

procedure Tiniciodesesion.BitBtn2Click(Sender: TObject);
begin
  Pasa := False;
  Close;
end;

procedure Tiniciodesesion.passwordKeyPress(Sender: TObject; var Key: Char);
begin
  If Key = #13 Then
    OkClick(Sender);
end;

end.
