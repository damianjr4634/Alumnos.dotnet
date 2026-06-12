SET TERM ^ ;

ALTER PROCEDURE WEB_NET_NOTASMATERIAS (IDALUMNO INTEGER,
CODALU VARCHAR(13) CHARACTER SET NONE,
IDCARRERA VARCHAR(6) CHARACTER SET NONE,
IDMATERIA VARCHAR(2) CHARACTER SET NONE)
RETURNS (NOTA1 NUMERIC(5, 2),
NOTA2 NUMERIC(5, 2),
NOTA3 NUMERIC(5, 2),
RECUP NUMERIC(5, 2),
MARZO NUMERIC(5, 2),
DICIE NUMERIC(5, 2),
INASI NUMERIC(5, 2),
JUSTI NUMERIC(5, 2),
FERRMSG VARCHAR(200) CHARACTER SET NONE)
AS 
begin
  FERRMSG='';

  select C.tp_eva, C.tp_eva2, C.tp_eva3, C.recup, C.notamar, C.notadic, c.inasist, c.justif
  from cursada C
  WHERE C.COD_ALU = :CODALU AND C.CARRE=:IDCARRERA AND C.cod_mat = :IDMATERIA
  INTO :NOTA1, :NOTA2, :NOTA3, :recup, :marzo, :dicie, :inasi, :justi;

  if (nota1 is null and nota2 is null) then begin
    select C.tp_eva, C.tp_eva2, C.tp_eva3, C.recup, C.notamar, C.notadic, c.inasist, c.justif
    from cursada_hst C
    WHERE C.COD_ALU = :CODALU AND C.CARRE=:IDCARRERA AND C.cod_mat = :IDMATERIA
    order by c.indice
    rows 1
    INTO :NOTA1, :NOTA2, :NOTA3, :recup, :marzo, :dicie, :inasi, :justi;

  end

  SUSPEND;
end ^

SET TERM ; ^
