program Project1;

uses
  Forms,
  CnfMail in 'CnfMail.pas' {FrmCnfMail};

{$R *.res}

begin
  Application.Initialize;
  Application.CreateForm(TFrmCnfMail, FrmCnfMail);
  Application.Run;
end.
