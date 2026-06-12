program Project1;

uses
  Forms,
  lstplanasis in 'lstplanasis.pas' {Frmlstplanasis},
  Imprimir in '..\Formulario Impresion\Imprimir.pas',
  Funciones in '..\ESBA\Funciones.pas',
  lstcomminis in '..\Formulario Listado Comisiones al Ministerio\lstcomminis.pas' {Frmlstcomminis},
  lstdocgral in '..\Formulario Listado Documentacion Gral\lstdocgral.pas' {Frmlstdocgral};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFrmlstplanasis, Frmlstplanasis);
  Application.CreateForm(TFrmlstcomminis, Frmlstcomminis);
  Application.CreateForm(TFrmlstdocgral, Frmlstdocgral);
  Application.Run;
end.
