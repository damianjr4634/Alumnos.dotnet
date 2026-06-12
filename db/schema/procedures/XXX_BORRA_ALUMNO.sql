SET TERM ^ ;

ALTER PROCEDURE XXX_BORRA_ALUMNO (CARRE VARCHAR(10) CHARACTER SET NONE,
CODALU VARCHAR(20) CHARACTER SET NONE,
USUARIO INTEGER)
RETURNS (FERRCOD INTEGER,
FERRMSG VARCHAR(1000) CHARACTER SET NONE)
AS 
declare variable SUPERV char(1);
begin
  SELECT U.superv
  FROM usuarios U
  WHERE U.codusu=:usuario
  INTO :SUPERV;
  if (SUPERV='N') then BEGIN
     FERRCOD=2;
     FERRMSG='Solo un supervisor puede borrar a un alumno. Avise a rectoria';
     suspend;
     exit;
  END
  FERRCOD=1;
  FERRMSG= 'Esta operacion borra a un alumno y todo su historial. Si continua no se puede deshacer esta opcion. Esta completamente seguro?';

  delete from analitic where cod_alu=:codalu and carre=:carre;
  delete from cursada where cod_alu=:codalu and carre=:carre;
  delete from permexa where cod_alu=:codalu and carre=:carre;
  delete from alumnos where cod_alu=:codalu and carre=:carre;
  delete from FALTAS where codalu=:codalu and carrera=:carre;
  suspend;
end ^

SET TERM ; ^
