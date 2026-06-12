unit ElimProfes;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons;

type
  TEliminacionProfes = class(TForm)
    Label1: TLabel;
    Eliminar: TBitBtn;
    BitBtn2: TBitBtn;
    Codigo: TComboBox;
    procedure CodigoKeyPress(Sender: TObject; var Key: Char);
    procedure BitBtn2Click(Sender: TObject);
    procedure EliminarClick(Sender: TObject);
    procedure FormActivate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  VUsuario : String;
  Vunidad  : String;
  EliminacionProfes: TEliminacionProfes;

implementation

{$R *.DFM}

procedure TEliminacionProfes.CodigoKeyPress(Sender: TObject; var Key: Char);
begin
 If Not(charinset(key,['0'..'9',#8])) Then
    Key := #0;
 If Key = chr(13) Then
   EliminarClick(Sender);
end;

procedure TEliminacionProfes.BitBtn2Click(Sender: TObject);
begin
// Profesores.Active := False;
 Close;
end;

procedure TEliminacionProfes.EliminarClick(Sender: TObject);
begin
 {If Profesores.FindKey([Copy(Codigo.Text,1,3)]) Then
  Begin
   If MessageBox(EliminacionProfes.Handle,Pchar(VUsuario+', estas segura de borrar al docente,'+ProfesoresDocente.Value),'Aviso',MB_ICONQUESTION+MB_APPLMODAL+MB_OKCANCEL) = IDOK Then
     Begin
      Profesores.delete;
      Codigo.Text := '';
      Codigo.Items.Delete(Codigo.ItemIndex);
      MessageBox(EliminacionProfes.Handle,Pchar(VUsuario+', el docente fué eliminado'),'Aviso',MB_ICONINFORMATION+MB_APPLMODAL+MB_OK);
     End;
   End
 Else
   MessageBox(EliminacionProfes.Handle,Pchar(VUsuario+', el codigo no existe'),'Aviso',MB_ICONINFORMATION+MB_APPLMODAL+MB_OK);}
end;

procedure TEliminacionProfes.FormActivate(Sender: TObject);
begin
{//BORRAR
  VUsuario := 'Rosa';
  Vunidad := 'G:\';}
{  Profesores.DatabaseName := VUnidad + 'New Pailot\Dbfgral\';
  Profesores.TableName := 'Docentes.Dbf';
  Profesores.IndexFieldNames := 'CODPROFES';
  Profesores.Active := True;
  While (Not Profesores.Eof) Do
   Begin
     Codigo.Items.Add(ProfesoresCodProfes.Value+ '-' + ProfesoresDocente.Value);
     Profesores.Next;
   End;}
end;

end.
