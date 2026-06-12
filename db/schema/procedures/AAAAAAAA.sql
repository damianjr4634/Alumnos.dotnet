SET TERM ^ ;

ALTER PROCEDURE AAAAAAAA AS 
begin
  /* paso codigos a otro numero */
update materias m set m.codmateri='90' where m.codmateri='08' and m.codcarre='CISE22';
update materias m set m.codmateri='91' where m.codmateri='11' and m.codcarre='CISE22';
update materias m set m.codmateri='92' where m.codmateri='09' and m.codcarre='CISE22';
update materias m set m.codmateri='93' where m.codmateri='10' and m.codcarre='CISE22';
update materias m set m.codmateri='94' where m.codmateri='15' and m.codcarre='CISE22';
update materias m set m.codmateri='95' where m.codmateri='06' and m.codcarre='CISE22';
update materias m set m.codmateri='96' where m.codmateri='07' and m.codcarre='CISE22';
update materias m set m.codmateri='97' where m.codmateri='12' and m.codcarre='CISE22';
update materias m set m.codmateri='98' where m.codmateri='13' and m.codcarre='CISE22';
update materias m set m.codmateri='99' where m.codmateri='14' and m.codcarre='CISE22';

update analitic a set a.cod_mat='90' where a.cod_mat='08' and a.carre='CISE22';
update analitic a set a.cod_mat='91' where a.cod_mat='11' and a.carre='CISE22';
update analitic a set a.cod_mat='92' where a.cod_mat='09' and a.carre='CISE22';
update analitic a set a.cod_mat='93' where a.cod_mat='10' and a.carre='CISE22';
update analitic a set a.cod_mat='94' where a.cod_mat='15' and a.carre='CISE22';
update analitic a set a.cod_mat='95' where a.cod_mat='06' and a.carre='CISE22';
update analitic a set a.cod_mat='96' where a.cod_mat='07' and a.carre='CISE22';
update analitic a set a.cod_mat='97' where a.cod_mat='12' and a.carre='CISE22';
update analitic a set a.cod_mat='98' where a.cod_mat='13' and a.carre='CISE22';
update analitic a set a.cod_mat='99' where a.cod_mat='14' and a.carre='CISE22';

update cursada c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update cursada c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update cursada c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update cursada c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update cursada c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update cursada c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update cursada c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update cursada c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update cursada c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update cursada c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';


update cursada_hst c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update cursada_hst c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';

update log_analitic c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update log_analitic c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update log_analitic c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update log_analitic c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update log_analitic c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update log_analitic c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update log_analitic c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update log_analitic c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update log_analitic c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update log_analitic c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';

update log_cursada c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update log_cursada c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update log_cursada c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update log_cursada c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update log_cursada c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update log_cursada c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update log_cursada c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update log_cursada c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update log_cursada c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update log_cursada c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';

update mesas c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update mesas c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update mesas c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update mesas c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update mesas c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update mesas c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update mesas c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update mesas c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update mesas c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update mesas c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';

update permexa c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update permexa c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update permexa c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update permexa c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update permexa c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update permexa c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update permexa c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update permexa c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update permexa c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update permexa c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';

update permexa_log c set c.cod_mat='90' where c.cod_mat='08' and c.carre='CISE22';
update permexa_log c set c.cod_mat='91' where c.cod_mat='11' and c.carre='CISE22';
update permexa_log c set c.cod_mat='92' where c.cod_mat='09' and c.carre='CISE22';
update permexa_log c set c.cod_mat='93' where c.cod_mat='10' and c.carre='CISE22';
update permexa_log c set c.cod_mat='94' where c.cod_mat='15' and c.carre='CISE22';
update permexa_log c set c.cod_mat='95' where c.cod_mat='06' and c.carre='CISE22';
update permexa_log c set c.cod_mat='96' where c.cod_mat='07' and c.carre='CISE22';
update permexa_log c set c.cod_mat='97' where c.cod_mat='12' and c.carre='CISE22';
update permexa_log c set c.cod_mat='98' where c.cod_mat='13' and c.carre='CISE22';
update permexa_log c set c.cod_mat='99' where c.cod_mat='14' and c.carre='CISE22';

--reordeno numeros

/*update materias m set m.codmateri='08', m.cuatrim=2 where m.codmateri='90' and m.codcarre='CISE22';
update materias m set m.codmateri='11', m.cuatrim=3 where m.codmateri='91' and m.codcarre='CISE22';
update materias m set m.codmateri='09', m.cuatrim=2 where m.codmateri='92' and m.codcarre='CISE22';
update materias m set m.codmateri='10', m.cuatrim=2 where m.codmateri='93' and m.codcarre='CISE22';
update materias m set m.codmateri='15', m.cuatrim=3 where m.codmateri='94' and m.codcarre='CISE22';
update materias m set m.codmateri='06', m.cuatrim=2 where m.codmateri='95' and m.codcarre='CISE22';
update materias m set m.codmateri='07', m.cuatrim=2 where m.codmateri='96' and m.codcarre='CISE22';
update materias m set m.codmateri='12', m.cuatrim=3 where m.codmateri='97' and m.codcarre='CISE22';
update materias m set m.codmateri='13', m.cuatrim=3 where m.codmateri='98' and m.codcarre='CISE22';
update materias m set m.codmateri='14', m.cuatrim=3 where m.codmateri='99' and m.codcarre='CISE22';

update analitic a set a.cod_mat='08' where a.cod_mat='90' and a.carre='CISE22';
update analitic a set a.cod_mat='11' where a.cod_mat='91' and a.carre='CISE22';
update analitic a set a.cod_mat='09' where a.cod_mat='92' and a.carre='CISE22';
update analitic a set a.cod_mat='10' where a.cod_mat='93' and a.carre='CISE22';
update analitic a set a.cod_mat='15' where a.cod_mat='94' and a.carre='CISE22';
update analitic a set a.cod_mat='06' where a.cod_mat='95' and a.carre='CISE22';
update analitic a set a.cod_mat='07' where a.cod_mat='96' and a.carre='CISE22';
update analitic a set a.cod_mat='12' where a.cod_mat='97' and a.carre='CISE22';
update analitic a set a.cod_mat='13' where a.cod_mat='98' and a.carre='CISE22';
update analitic a set a.cod_mat='14' where a.cod_mat='99' and a.carre='CISE22';

update cursada c set c.cod_mat='08' where c.cod_mat='90' and c.carre='CISE22';
update cursada c set c.cod_mat='11' where c.cod_mat='91' and c.carre='CISE22';
update cursada c set c.cod_mat='09' where c.cod_mat='92' and c.carre='CISE22';
update cursada c set c.cod_mat='10' where c.cod_mat='93' and c.carre='CISE22';
update cursada c set c.cod_mat='15' where c.cod_mat='94' and c.carre='CISE22';
update cursada c set c.cod_mat='06' where c.cod_mat='95' and c.carre='CISE22';
update cursada c set c.cod_mat='07' where c.cod_mat='96' and c.carre='CISE22';
update cursada c set c.cod_mat='12' where c.cod_mat='97' and c.carre='CISE22';
update cursada c set c.cod_mat='13' where c.cod_mat='98' and c.carre='CISE22';
update cursada c set c.cod_mat='14' where c.cod_mat='99' and c.carre='CISE22';

update cursada_hst ch set ch.cod_mat='08' where ch.cod_mat='90' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='11' where ch.cod_mat='91' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='09' where ch.cod_mat='92' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='10' where ch.cod_mat='93' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='15' where ch.cod_mat='94' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='06' where ch.cod_mat='95' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='07' where ch.cod_mat='96' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='12' where ch.cod_mat='97' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='13' where ch.cod_mat='98' and ch.carre='CISE22';
update cursada_hst ch set ch.cod_mat='14' where ch.cod_mat='99' and ch.carre='CISE22';

update log_analitic a set a.cod_mat='08' where a.cod_mat='90' and a.carre='CISE22';
update log_analitic a set a.cod_mat='11' where a.cod_mat='91' and a.carre='CISE22';
update log_analitic a set a.cod_mat='09' where a.cod_mat='92' and a.carre='CISE22';
update log_analitic a set a.cod_mat='10' where a.cod_mat='93' and a.carre='CISE22';
update log_analitic a set a.cod_mat='15' where a.cod_mat='94' and a.carre='CISE22';
update log_analitic a set a.cod_mat='06' where a.cod_mat='95' and a.carre='CISE22';
update log_analitic a set a.cod_mat='07' where a.cod_mat='96' and a.carre='CISE22';
update log_analitic a set a.cod_mat='12' where a.cod_mat='97' and a.carre='CISE22';
update log_analitic a set a.cod_mat='13' where a.cod_mat='98' and a.carre='CISE22';
update log_analitic a set a.cod_mat='14' where a.cod_mat='99' and a.carre='CISE22';

update log_cursada c set c.cod_mat='08' where c.cod_mat='90' and c.carre='CISE22';
update log_cursada c set c.cod_mat='11' where c.cod_mat='91' and c.carre='CISE22';
update log_cursada c set c.cod_mat='09' where c.cod_mat='92' and c.carre='CISE22';
update log_cursada c set c.cod_mat='10' where c.cod_mat='93' and c.carre='CISE22';
update log_cursada c set c.cod_mat='15' where c.cod_mat='94' and c.carre='CISE22';
update log_cursada c set c.cod_mat='06' where c.cod_mat='95' and c.carre='CISE22';
update log_cursada c set c.cod_mat='07' where c.cod_mat='96' and c.carre='CISE22';
update log_cursada c set c.cod_mat='12' where c.cod_mat='97' and c.carre='CISE22';
update log_cursada c set c.cod_mat='13' where c.cod_mat='98' and c.carre='CISE22';
update log_cursada c set c.cod_mat='14' where c.cod_mat='99' and c.carre='CISE22';

update mesas m set m.cod_mat='08' where m.cod_mat='90' and m.carre='CISE22';
update mesas m set m.cod_mat='11' where m.cod_mat='91' and m.carre='CISE22';
update mesas m set m.cod_mat='09' where m.cod_mat='92' and m.carre='CISE22';
update mesas m set m.cod_mat='10' where m.cod_mat='93' and m.carre='CISE22';
update mesas m set m.cod_mat='15' where m.cod_mat='94' and m.carre='CISE22';
update mesas m set m.cod_mat='06' where m.cod_mat='95' and m.carre='CISE22';
update mesas m set m.cod_mat='07' where m.cod_mat='96' and m.carre='CISE22';
update mesas m set m.cod_mat='12' where m.cod_mat='97' and m.carre='CISE22';
update mesas m set m.cod_mat='13' where m.cod_mat='98' and m.carre='CISE22';
update mesas m set m.cod_mat='14' where m.cod_mat='99' and m.carre='CISE22';

update permexa m set m.cod_mat='08' where m.cod_mat='90' and m.carre='CISE22';
update permexa m set m.cod_mat='11' where m.cod_mat='91' and m.carre='CISE22';
update permexa m set m.cod_mat='09' where m.cod_mat='92' and m.carre='CISE22';
update permexa m set m.cod_mat='10' where m.cod_mat='93' and m.carre='CISE22';
update permexa m set m.cod_mat='15' where m.cod_mat='94' and m.carre='CISE22';
update permexa m set m.cod_mat='06' where m.cod_mat='95' and m.carre='CISE22';
update permexa m set m.cod_mat='07' where m.cod_mat='96' and m.carre='CISE22';
update permexa m set m.cod_mat='12' where m.cod_mat='97' and m.carre='CISE22';
update permexa m set m.cod_mat='13' where m.cod_mat='98' and m.carre='CISE22';
update permexa m set m.cod_mat='14' where m.cod_mat='99' and m.carre='CISE22';
                                                     
update permexa_log m set m.cod_mat='08' where m.cod_mat='90' and m.carre='CISE22';
update permexa_log m set m.cod_mat='11' where m.cod_mat='91' and m.carre='CISE22';
update permexa_log m set m.cod_mat='09' where m.cod_mat='92' and m.carre='CISE22';
update permexa_log m set m.cod_mat='10' where m.cod_mat='93' and m.carre='CISE22';
update permexa_log m set m.cod_mat='15' where m.cod_mat='94' and m.carre='CISE22';
update permexa_log m set m.cod_mat='06' where m.cod_mat='95' and m.carre='CISE22';
update permexa_log m set m.cod_mat='07' where m.cod_mat='96' and m.carre='CISE22';
update permexa_log m set m.cod_mat='12' where m.cod_mat='97' and m.carre='CISE22';
update permexa_log m set m.cod_mat='13' where m.cod_mat='98' and m.carre='CISE22';
update permexa_log m set m.cod_mat='14' where m.cod_mat='99' and m.carre='CISE22';*/
end ^

SET TERM ; ^
