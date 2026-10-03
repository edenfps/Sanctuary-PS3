<?php

file_put_contents("create.log", var_export($_POST, true), FILE_APPEND);

header('Content-type: text/xml');

echo '<CharacterCreateReply Status="1" StatusMessage="Test">
	<Guid>12345</Guid>
</CharacterCreateReply>';

?>