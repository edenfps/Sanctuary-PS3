<?php

file_put_contents("info.log", var_export($_POST, true), FILE_APPEND);

header('Content-type: text/xml');

echo '<AccountInfoReply Status="2" StatusMessage="Test">
	<Account Member="1" MaxCharacters="10" Admin="1" />
</AccountInfoReply>';

?>