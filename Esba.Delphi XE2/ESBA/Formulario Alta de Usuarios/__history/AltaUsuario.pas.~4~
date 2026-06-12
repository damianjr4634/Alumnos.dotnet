unit AltaUsuario;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, FuncionesText, Db, DataModule;

type
  TFrmAltaUsuario = class(TForm)
    Graba: TBitBtn;
    Salir: TBitBtn;
    GroupBox1: TGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    RPassword: TEdit;
    Password: TEdit;
    Usuario: TEdit;
    GroupBox2: TGroupBox;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    Cargo: TEdit;
    Apellido: TEdit;
    Nombre: TEdit;
    procedure GrabaClick(Sender: TObject);
    procedure SalirClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FrmAltaUsuario: TFrmAltaUsuario;
  VUnidad : String;

implementation

{$R *.DFM}

procedure TFrmAltaUsuario.GrabaClick(Sender: TObject);
begin
If (Password.Text <> '') And (Rpassword.Text <> '') And (Usuario.Text <> '') Then
 Begin
  If (Password.Text = RPassword.Text) Then
   Begin
      With DataModule.CustomerData do
        begin
          if trqrvarios.Active then
              trqrvarios.Rollback;
          trqrvarios.Active:=False;
          IBQrVarios.SQL.Clear;
          IBQrVarios.SQL.Text:='SELECT CARGO FROM USUARIOS WHERE NOMBRE=:NOMBRE';
          IBQrVarios.ParamByName('NOMBRE').Value:=Usuario.Text;
          TrQrVarios.Active:=True;
          IbQrVarios.ExecQuery;
          If (IbQrVarios.FieldByName('CARGO').AsString = '') Then
            begin
               IbUpVarios.Sql.Clear;
               TrupVarios.Active:=False;
               IbUpVarios.SQL.Text:='INSERT INTO USUARIOS(CARGO,PASSWD,NOMBRE,APELLIDO,NOMUSU) VALUES(:CARGO,:PASWD,:NOMBRE,:APELLIDO,:NOMUSU);';
               IbupVarios.ParamByName('CARGO').Value:=Cargo.Text;
               IbupVarios.ParamByName('PASWD').Value:=encriptocadena2(Password.Text,1);
               IbupVarios.ParamByName('NOMBRE').Value:=Usuario.Text;
               IbupVarios.ParamByName('APELLIDO').Value:=Apellido.Text;
               IbupVarios.ParamByName('CARGO').Value:=Cargo.Text;
               IbupVarios.ParamByName('NOMUSU').Value:=NOMBRE.Text;
               TrupVarios.Active:=True;
               IbUpVarios.ExecQuery;
               TrUpVarios.Commit;
            end
          Else
            MessageDlg('El nombre de usuario ya existe ingrese otro.',mtError,[mbok],0,mbok);
          TrQrVarios.Rollback;
          TrQrVarios.Active:=False;
        end;
         Usuario.Text := '';
         Nombre.Text := '';
         Apellido.Text := '';
         Cargo.Text := '';
         Password.Text := '';
         RPassword.Text := '';
   End
  Else
    MessageDlg('Los password ingresados no son iguales.',mtError,[mbok],0,mbok);
 End
Else
  MessageDlg('Debe haber un nombre de usario y/o password.',mtError,[mbok],0,mbok);
end;

procedure TFrmAltaUsuario.SalirClick(Sender: TObject);
begin
  Close;
end;

end.
