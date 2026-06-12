SET TERM ^ ;

ALTER PROCEDURE WEB_NET_LOGIN (MAIL VARCHAR(300) CHARACTER SET NONE,
TIPOUSUARIO CHAR(1) CHARACTER SET NONE,
LOGIN INTEGER)
RETURNS (FERRMSG VARCHAR(100) CHARACTER SET NONE,
COD_ALU VARCHAR(13) CHARACTER SET NONE,
ID_ALUMNO INTEGER,
DISTANCIA CHAR(1) CHARACTER SET NONE,
NOMBRE VARCHAR(200) CHARACTER SET NONE,
CARRE VARCHAR(6) CHARACTER SET NONE,
DESCARRE VARCHAR(200) CHARACTER SET NONE,
BAJA CHAR(1) CHARACTER SET NONE,
TIPO_CARRERA VARCHAR(3) CHARACTER SET NONE)
AS 
begin
  FERRMSG='';

  IF (TIPOUSUARIO='A') THEN BEGIN
     FOR SELECT A.indice, A.baja, COD_ALU, TRIM(A.APELLIDO)||', '||TRIM(A.NOM_APE), A.CARRE, C.descarre, c.distancia,
         C.tipo
     FROM ALUMNOS A
     LEFT OUTER JOIN CARRERA C ON C.carre= A.CARRE
     WHERE A.mail=:MAIL and a.fusuweb = 'S'
     INTO :ID_ALUMNO, :BAJA, :COD_ALU, :NOMBRE, :CARRE, :descarre, :distancia, :tipo_carrera
     DO BEGIN
        SUSPEND;
        if (LOGIN = 0) then
            EXIT;
     END
  END
  else IF (TIPOUSUARIO='P') THEN BEGIN
     FOR SELECT A.indice, A.baja, a.COD_ALU, TRIM(A.APELLIDO)||', '||TRIM(A.NOM_APE), A.CARRE, C.descarre, c.distancia, C.tipo
     FROM tutores t
     left outer join ALUMNOS A on a.cod_alu=t.cod_alu and a.carre=t.carre
     LEFT OUTER JOIN CARRERA C ON C.carre= A.CARRE
     WHERE T.femail=:MAIL and T.fusuweb = 'S'
     INTO :ID_ALUMNO, :BAJA, :COD_ALU, :NOMBRE, :CARRE, :descarre, :distancia, :tipo_carrera
     DO BEGIN
        SUSPEND;
        if (LOGIN = 0) then
            EXIT;
     END
  end
end ^

SET TERM ; ^
