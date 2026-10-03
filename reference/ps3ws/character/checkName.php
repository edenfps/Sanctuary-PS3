<?php

file_put_contents("checkName.log", var_export($_POST, true), FILE_APPEND);

header('Content-type: text/xml');

echo '<CheckNameReply Status="1" StatusMessage="Test">
	<Suggestions>
		<Name FirstName="EDITz" LastNamePrefix="1" LastNameSuffix="2" />
	</Suggestions>
</CheckNameReply>';

?>