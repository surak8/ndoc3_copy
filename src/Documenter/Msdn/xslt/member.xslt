<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ndoc="urn:ndoc-schema"
                xmlns:NUtil="urn:NDocUtil"
	              exclude-result-prefixes="NUtil">
	<!-- -->
	<xsl:include href="common.xslt" />
	<!-- -->
	<xsl:param name='assembly-name' />
	<xsl:param name='member-id' />
	<!-- -->
	<xsl:template match="/">
		<xsl:apply-templates select="ndoc:ndoc/ndoc:assembly[@name=$assembly-name]/ndoc:module/ndoc:namespace/ndoc:*/ndoc:*[@id=$member-id][1]" />
	</xsl:template>

	<xsl:template name="FormatMemberDisplay">
		<xsl:param name="memberName" />
		<xsl:param name="memberType" />
		<xsl:value-of select="NUtil:ToGeneralGenericFormat(../@displayName)" />
		<xsl:if test="local-name()='method'">
			<xsl:text>.</xsl:text>
			<xsl:value-of select="NUtil:ToGeneralGenericFormat(@displayName)" />
		</xsl:if>
		<xsl:if test="local-name()!='operator'">
			<xsl:if test="count(parent::node()/*[@name=$memberName]) > 1">
				<xsl:call-template name="get-param-list" />
			</xsl:if>
		</xsl:if>
		<xsl:text>&#32;</xsl:text>
		<xsl:value-of select="$memberType" />
		
	</xsl:template>

	<!-- Method, contructor or opreator overload -->
	<xsl:template match="ndoc:method | ndoc:constructor | ndoc:operator">
		<xsl:variable name="type">
			<xsl:choose>
				<xsl:when test="local-name(..)='interface'">Interface</xsl:when>
				<xsl:otherwise>Class</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="childType">
			<xsl:choose>
				<xsl:when test="local-name()='method'">Method</xsl:when>
				<xsl:when test="local-name()='operator'">
					<xsl:call-template name="operator-name">
						<xsl:with-param name="name">
							<xsl:value-of select="@name" />
						</xsl:with-param>
						<xsl:with-param name="from">
							<xsl:value-of select="ndoc:parameter/@type" />
						</xsl:with-param>
						<xsl:with-param name="to">
							<xsl:value-of select="returnType" />
						</xsl:with-param>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@contract='Static'">Static Constructor</xsl:when>
				<xsl:otherwise>Constructor</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="memberName" select="NUtil:ToGeneralGenericFormat(@displayName)" />
		<html dir="LTR">
			<xsl:call-template name="html-head">
				<xsl:with-param name="title">
					<xsl:call-template name="FormatMemberDisplay">
						<xsl:with-param name="memberName" select="$memberName" />
						<xsl:with-param name="memberType" select="$childType" />
					</xsl:call-template>
				</xsl:with-param>
			</xsl:call-template>
			<body id="bodyID" class="dtBODY">
				<xsl:call-template name="title-row">
					<xsl:with-param name="type-name">
						<xsl:call-template name="FormatMemberDisplay">
							<xsl:with-param name="memberName" select="$memberName" />
							<xsl:with-param name="memberType" select="$childType" />
						</xsl:call-template>
					</xsl:with-param>
				</xsl:call-template>
				<div id="nstext">
					<xsl:call-template name="summary-section" />
					<xsl:call-template name="vb-member-syntax" />
					<xsl:call-template name="cs-member-syntax" />
					<xsl:call-template name="parameter-section" />
					<xsl:call-template name="returnvalue-section" />
					<xsl:call-template name="implements-section" />
					<xsl:call-template name="remarks-section" />
					<xsl:apply-templates select="ndoc:documentation/node()" mode="after-remarks-section" />
					<xsl:call-template name="events-section" />
					<xsl:call-template name="exceptions-section" />
					<xsl:call-template name="example-section" />
					<xsl:call-template name="requirements-section" />
					<xsl:call-template name="seealso-section">
						<xsl:with-param name="page">member</xsl:with-param>
					</xsl:call-template>

					<xsl:if test="local-name()='constructor'">
						<xsl:if test="count(parent::node()/constructor) &lt; 2">
							<xsl:if test="not($ndoc-omit-object-tags)">
								<object type="application/x-oleobject" classid="clsid:1e2a7bd0-dab9-11d0-b93a-00c04fc99f9e" viewastext="true" style="display: none;">
									<xsl:element name="param">
										<xsl:attribute name="name">Keyword</xsl:attribute>
										<xsl:attribute name="value">
											<xsl:value-of select='../@name' /> class, constructor
										</xsl:attribute>
									</xsl:element>
								</object>
							</xsl:if>
						</xsl:if>
					</xsl:if>

					<xsl:if test="local-name()='method'">
						<xsl:if test="not($ndoc-omit-object-tags)">
							<object type="application/x-oleobject" classid="clsid:1e2a7bd0-dab9-11d0-b93a-00c04fc99f9e" viewastext="true" style="display: none;">
								<xsl:element name="param">
									<xsl:attribute name="name">Keyword</xsl:attribute>
									<xsl:attribute name="value">
										<xsl:value-of select='@name' /> method
									</xsl:attribute>
								</xsl:element>
								<xsl:element name="param">
									<xsl:attribute name="name">Keyword</xsl:attribute>
									<xsl:attribute name="value">
										<xsl:value-of select="concat(@name, ' method, ', ../@name, ' ', local-name(parent::*))" />
									</xsl:attribute>
								</xsl:element>
								<xsl:element name="param">
									<xsl:attribute name="name">Keyword</xsl:attribute>
									<xsl:attribute name="value">
										<xsl:value-of select='../@name' />.<xsl:value-of select='@name' /> method
									</xsl:attribute>
								</xsl:element>
							</object>
						</xsl:if>
					</xsl:if>

					<xsl:call-template name="footer-row">
						<xsl:with-param name="type-name">
							<xsl:value-of select="../@name" />
							<xsl:if test="local-name()='method'">
								<xsl:text>.</xsl:text>
								<xsl:value-of select="@name" />
							</xsl:if>
							<xsl:text>&#160;</xsl:text>
							<xsl:value-of select="$childType" />
							<xsl:text>&#160;</xsl:text>
							<xsl:if test="local-name()!='operator'">
								<xsl:if test="count(parent::node()/*[@name=$memberName]) &gt; 1">
									<xsl:call-template name="get-param-list" />
								</xsl:if>
							</xsl:if>
						</xsl:with-param>
					</xsl:call-template>
				</div>
			</body>
		</html>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
